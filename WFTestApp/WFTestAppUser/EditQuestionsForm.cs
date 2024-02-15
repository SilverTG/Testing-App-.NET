using DBClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace WFTestAppUser
{
    public partial class EditQuestionsForm : Form
    {
        public string connectStr = ConfigurationManager.ConnectionStrings["connectStr"].ConnectionString;
        public List<Question> questions = new List<Question>();
        public List<Answer> answers = new List<Answer>();
        public EditQuestionsForm(List<Question> q,List<Answer> a)
        {
            questions = q;
            answers = a;
            InitializeComponent();
        }
        private void btnReturn_Click(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms)
            {
                if (form != this && !form.Visible)
                {
                    form.Show();
                    break;
                }
            }
            this.Close();
        }
        private void btnDeleteQuestion_Click(object sender, EventArgs e)
        {
            bool deleted = false;
            Question delQuestion = new Question();
            int idDel = Convert.ToInt32(nudQuestionIdToDel.Value);
            for (int i = 0; i < questions.Count; i++)
            {
                if (questions[i].QuestionID == idDel)
                {
                    delQuestion = questions[i];
                    questions.RemoveAt(i);
                    deleted = true;
                    string sqlQuery = $"DELETE FROM Questions WHERE QuestionID = {delQuestion.QuestionID}; DELETE FROM Answers WHERE QuestionID = {delQuestion.QuestionID}";

                    using (SqlConnection connection = new SqlConnection(connectStr))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                        {
                            command.ExecuteNonQuery();
                        }
                        connection.Close();
                    }
                    break;
                }
            }
            if (deleted) MessageBox.Show($"Deleted question:\nID: {delQuestion.QuestionID}\nQuestion: {delQuestion.QuestionText}\nImage Link: {delQuestion.ImageLink}");
            else MessageBox.Show($"Question with ID:{idDel} is not found!");
        }

        private void btnAddQuestion_Click(object sender, EventArgs e)
        {
            if (CheckIfAllTextBoxesFilled() && CheckIfAnyCheckBoxChecked())
            {
                string questionQuery = $"INSERT Questions (QuestionText, ImageLink) VALUES ('{tbQuestion.Text}', '{tbImageLink.Text}'); SELECT SCOPE_IDENTITY();";
                int questionId;
                using (SqlConnection connection = new SqlConnection(connectStr))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(questionQuery, connection))
                    {
                        questionId = Convert.ToInt32(command.ExecuteScalar());
                    }
                    connection.Close();
                }
                // Insert the answers using the questionId
                string answersQuery = $"INSERT INTO Answers (QuestionID, AnswerText, IsCorrectAnswer) VALUES " +
                                     $"({questionId}, '{tbAnswer1.Text}', {Convert.ToByte(cbIsRight1.Checked)})," +
                                     $"({questionId}, '{tbAnswer2.Text}', {Convert.ToByte(cbIsRight2.Checked)})," +
                                     $"({questionId}, '{tbAnswer3.Text}', {Convert.ToByte(cbIsRight3.Checked)})," +
                                     $"({questionId}, '{tbAnswer4.Text}', {Convert.ToByte(cbIsRight4.Checked)});";
                using (SqlConnection connection = new SqlConnection(connectStr))
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(answersQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                    MessageBox.Show("Question Added Succesfull!");
                    List<System.Windows.Forms.TextBox> textBoxes = new List<System.Windows.Forms.TextBox>
                        {
                            tbAnswer1,tbAnswer2, tbAnswer3, tbAnswer4,tbQuestion,tbImageLink
                        };
                    foreach (System.Windows.Forms.TextBox tb in textBoxes)
                    {
                        tb.Text = "";
                    }
                    cbIsRight1.Checked = false;
                    cbIsRight2.Checked = false;
                    cbIsRight3.Checked = false;
                    cbIsRight4.Checked = false;

                }
            }
        }
        private bool CheckIfAllTextBoxesFilled()
        {
            List<System.Windows.Forms.TextBox> textBoxes = new List<System.Windows.Forms.TextBox>
            {
                tbAnswer1,tbAnswer2, tbAnswer3, tbAnswer4,tbQuestion,tbImageLink
            };

            foreach (System.Windows.Forms.TextBox textBox in textBoxes)
            {
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    return false;
                }
            }
            return true;
        }
        private bool CheckIfAnyCheckBoxChecked()
        {
            List<System.Windows.Forms.CheckBox> checkboxes = new List<System.Windows.Forms.CheckBox>
            {
                cbIsRight1,cbIsRight2, cbIsRight3,cbIsRight4
            };

            foreach (System.Windows.Forms.CheckBox checkbox in checkboxes)
            {
                if (checkbox.Checked)
                {
                    return true; 
                }
            }
            return false;
        }
    }
}
