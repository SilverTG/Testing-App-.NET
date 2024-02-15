using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DBClasses;

namespace WFTestAppAdmin
{
    public partial class UserForm : Form
    {
        private TcpClient client;
        public List<Question> questions = new List<Question>();
        public List<Answer> answers = new List<Answer>();
        public int time = 10;
        public UserForm()
        {
            InitializeComponent();
        }
        private void ConnectToServer()
        {
            try
            {
                client = new TcpClient();
                client.Connect(IPAddress.Parse("127.0.0.1"), 1234);

                try
                {
                    NetworkStream stream = client.GetStream();

                    byte[] buffer = new byte[4]; // Buffer to store the header containing the length of the incoming data

                    //send name+ lastname
                    byte[] fullNameData = Encoding.UTF8.GetBytes(tbName.Text + " " +tbLastName.Text);
                    byte[] fullNameHeader = BitConverter.GetBytes(fullNameData.Length);
                    stream.Write(fullNameHeader, 0, fullNameHeader.Length);
                    stream.Write(fullNameData, 0, fullNameData.Length);
                    //receive time
                    byte[] timeHeader = new byte[4];
                    stream.Read(timeHeader, 0, timeHeader.Length);
                    int timeLength = BitConverter.ToInt32(timeHeader, 0);

                    byte[] timeData = new byte[timeLength];
                    stream.Read(timeData, 0, timeLength);
                    time = BitConverter.ToInt32(timeData, 0);
                   // MessageBox.Show(time.ToString());
                    // Receive data
                    while (true)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead < buffer.Length) break;
                        int dataLength = BitConverter.ToInt32(buffer, 0);
                        byte[] data = new byte[dataLength];
                        stream.Read(data, 0, dataLength);
                        string dataString = Encoding.UTF8.GetString(data);
                        //MessageBox.Show(dataString);
                        if (IsQuestionString(dataString))
                        {
                            Question question = Question.FromString(dataString);
                            questions.Add(question);
                        }
                        else
                        {
                            Answer answer = Answer.FromString(dataString);
                            answers.Add(answer);
                        }
                    }

                    TestingForm tf = new TestingForm(questions, answers, time, tbName.Text, tbLastName.Text);
                    this.Hide();
                    tf.Show();
                    stream.Close();
                    client.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Admin didnt start server, please try again later! \nError: " + ex.ToString());
            }
        }
        public static bool IsQuestionString(string dataString)
        {
            string[] values = dataString.Split('|');
            if (values.Length != 3) return false;
            return true;
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            if ((!string.IsNullOrEmpty(tbName.Text) && tbName.Text.Length > 2) && (!string.IsNullOrEmpty(tbLastName.Text) && tbLastName.Text.Length > 2))
            {
                ConnectToServer();
            }
            else MessageBox.Show("Please, fill all required fields");
        }

    }
}
