using DBClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static WFTestAppUser.MainAdminForm;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace WFTestAppUser
{
    public partial class MainAdminForm : Form
    {
            public string connectStr = ConfigurationManager.ConnectionStrings["connectStr"].ConnectionString;
            private TcpListener server;
            private TcpClient client;
            public List<User> users = new List<User>();
            public List<Question> questions = new List<Question>();
            public List<Answer> answers = new List<Answer>();
        public MainAdminForm()
        {
            InitializeComponent();
            Task.Run(LoadData);
        }
        
        private void LoadData()
            {
            users = new List<User>();
            questions = new List<Question>();
            answers = new List<Answer>();
                using (SqlConnection connection = new SqlConnection(connectStr))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand("SELECT * FROM Users", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                User user = new User
                                {
                                    UserID = (int)reader["UserID"],
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString(),
                                    NumberOfPassedTests = (int)reader["NumberOfPassedTests"],
                                    HighestScore = (int)reader["HighestScore"],
                                    LowestScore = (int)reader["LowestScore"]
                                };
                                users.Add(user);
                            }
                        }
                    }
                    using (SqlCommand command = new SqlCommand("SELECT * FROM Questions", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Question question = new Question
                                {
                                    QuestionID = (int)reader["QuestionID"],
                                    QuestionText = reader["QuestionText"].ToString(),
                                    ImageLink = reader["ImageLink"].ToString()
                                };
                                questions.Add(question);
                            }
                        }
                    }
                    using (SqlCommand command = new SqlCommand("SELECT * FROM Answers", connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Answer answer = new Answer
                                {
                                    AnswerID = (int)reader["AnswerID"],
                                    QuestionID = (int)reader["QuestionID"],
                                    AnswerText = reader["AnswerText"].ToString(),
                                    IsCorrectAnswer = (bool)reader["IsCorrectAnswer"]
                                };
                                answers.Add(answer);
                            }
                        }
                    }
                }

                dgvQuestions.Invoke(new Action(() =>
                {
                    dgvQuestions.DataSource = questions;
                    dgvQuestions.AutoGenerateColumns = true;
                    dgvQuestions.Columns[0].Width = 30;
                }));

                dgvAnswers.Invoke(new Action(() =>
                {
                    dgvAnswers.DataSource = answers;
                    dgvAnswers.AutoGenerateColumns = true;
                    dgvAnswers.Columns[0].Width = 30;
                    dgvAnswers.Columns[1].Width = 30;
                }));
            nudAmountOfQuestions.Invoke(new Action(() =>
            {
                nudAmountOfQuestions.Maximum = questions.Count;
            }));
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            int amountOfQuestions = Convert.ToInt32(nudAmountOfQuestions.Value);
            int testTime = Convert.ToInt32(nudTestTime.Value);

            nudAmountOfQuestions.Enabled = false;
            nudTestTime.Enabled = false;
            Task.Run(()=> StartServerAndSendData(amountOfQuestions, testTime));
        }
        private List<Question> MixedQuestions(int amountOfQuestions)
        {
            List<Question> duplicate = new List<Question>(questions);
            List<Question> mixedQuestions = new List<Question>();

            Random random = new Random();
            int questionCount = questions.Count;
            for (int i = 0; i<amountOfQuestions; i++)
            {
                int randomIndex = random.Next(questionCount);
                mixedQuestions.Add(duplicate[randomIndex]);
                duplicate.RemoveAt(randomIndex);
                questionCount--;
            }
            return mixedQuestions;
        }
        private void StartServerAndSendData(int amountOfQuestions,int testTime)
        {
            try
            {
                server = new TcpListener(IPAddress.Any, 1234);
                server.Start();
                while (true)
                {
                    client = server.AcceptTcpClient();
                    string ip = ((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString();
                    List<Question> mixedQuestions = MixedQuestions(amountOfQuestions);
                    string currentTime = DateTime.Now.ToString("HH:mm:ss");
                    
                    NetworkStream stream = client.GetStream();

                    //recieve name+lastName
                    byte[] headerBuffer = new byte[4];
                    stream.Read(headerBuffer, 0, 4);
                    int dataLength = BitConverter.ToInt32(headerBuffer, 0);
                    byte[] dataBuffer = new byte[dataLength];
                    stream.Read(dataBuffer, 0, dataLength);
                    string FullName = Encoding.UTF8.GetString(dataBuffer);
                    //status dvgStatus
                    dgvStatus.BeginInvoke(new Action(() =>
                    {
                        int rowIndex = 0;
                        rowIndex = dgvStatus.Rows.Add();
                        dgvStatus.Rows[rowIndex].Cells[0].Value = ip;
                        dgvStatus.Rows[rowIndex].Cells[1].Value = FullName;
                        dgvStatus.Rows[rowIndex].Cells[2].Value = currentTime;
                        dgvStatus.Rows[rowIndex].Cells[3].Value = "Started Test";
                    }));
                    //send time
                    byte[] timeData = BitConverter.GetBytes(testTime);
                    byte[] timeHeader = BitConverter.GetBytes(timeData.Length);
                    stream.Write(timeHeader, 0, timeHeader.Length);
                    stream.Write(timeData, 0, timeData.Length);
                    //send questions
                    foreach (Question question in mixedQuestions)
                    {
                        byte[] questionData = Encoding.UTF8.GetBytes(question.ToString());
                        // Send the length of the string as a 4-byte header
                        byte[] header = BitConverter.GetBytes(questionData.Length);
                        stream.Write(header, 0, header.Length);
                        // Send the string data
                        stream.Write(questionData, 0, questionData.Length);
                    }

                    //finding answers to mixedQuestions
                    List<Answer> mixedAnswers = new List<Answer>();
                    for (int i = 0; i < mixedQuestions.Count; i++)
                    {
                        for (int j = 0; j < answers.Count; j++)
                        {
                            if (mixedQuestions[i].QuestionID == answers[j].QuestionID)
                            {
                                mixedAnswers.Add(answers[j]);
                            }
                        }
                    }
                    // Send answers
                    foreach (Answer answer in mixedAnswers)
                    {
                        byte[] answerData = Encoding.UTF8.GetBytes(answer.ToString());
                        // Send the length of the string as a 4-byte header
                        byte[] header = BitConverter.GetBytes(answerData.Length);
                        stream.Write(header, 0, header.Length);
                        // Send the string data
                        stream.Write(answerData, 0, answerData.Length);
                    }

                    stream.Close();
                }
            }
            catch //(Exception ex)
            {
                BeginInvoke(new Action(() => MessageBox.Show("Server Stoped")));//"Error!\n"+ex.Message
                StopServer();
            }
        }

        private void btnEditQuestions_Click(object sender, EventArgs e)
        {
            EditQuestionsForm editQuestionsForm = new EditQuestionsForm(questions,answers);
            editQuestionsForm.FormClosed += EditQuestionsForm_FormClosed;
            editQuestionsForm.Show();
            this.Hide();
        }
        private void EditQuestionsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.Show();
            LoadData();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = true;
            btnStop.Enabled = false;
            nudAmountOfQuestions.Enabled = true;
            nudTestTime.Enabled = true;
            StopServer();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            StopServer();
            this.Close();
        }
        private void StopServer() 
        {
            client?.Close();
            server?.Stop();
        }

        private void nud_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown numericUpDown = (NumericUpDown)sender;
            if (numericUpDown.Value > numericUpDown.Maximum)
            {
                numericUpDown.Value = numericUpDown.Maximum;
            }
        }
    }
}
