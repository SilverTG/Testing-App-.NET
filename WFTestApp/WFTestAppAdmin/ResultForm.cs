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

namespace WFTestAppAdmin
{
    public partial class ResultForm : Form
    {
        private List<User> users = new List<User>();
        private string firstName;
        private string lastName;
        private int score;
        private int time;
        public string connectStr = ConfigurationManager.ConnectionStrings["connectStr"].ConnectionString;
        public ResultForm(string fn,string ln,int s,int t)
        {
            firstName = fn;
            lastName = ln;
            score = s;
            time = t;
            InitializeComponent();
        }
        private void ResultForm_Load(object sender, EventArgs e)
        {
            lbUserName.Text = firstName+" "+lastName;
            lbScore.Text = score.ToString();
            TimeSpan timeSpan = TimeSpan.FromSeconds(time);
            lbTime.Text = timeSpan.ToString(@"mm\:ss");

            using (SqlConnection connection = new SqlConnection(connectStr))
            {
                connection.Open();

                // check if the user exists
                string query = $"SELECT COUNT(*) FROM Users WHERE FirstName = '{firstName}' AND LastName = '{lastName}'";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    int count = (int)command.ExecuteScalar();
                    if (count == 0)
                    {
                        // If the user does not exist, add a new row
                        query = $"INSERT INTO Users (FirstName, LastName,NumberOfPassedTests,HighestScore,LowestScore) VALUES ('{firstName}', '{lastName}',1,{score},{score});";
                    }
                    else
                    {
                        // If the user exists, update the existing row
                        query = $"UPDATE Users SET NumberOfPassedTests = NumberOfPassedTests + 1, " +
                                $"HighestScore = CASE WHEN {score} > HighestScore THEN {score} ELSE HighestScore END, " +
                                $"LowestScore = CASE WHEN {score} < LowestScore THEN {score} ELSE LowestScore END " +
                                $"WHERE FirstName = '{firstName}' AND LastName = '{lastName}';";
                    }
                    using (SqlCommand insertUpdateCommand = new SqlCommand(query, connection))
                    {
                        insertUpdateCommand.ExecuteNonQuery();
                    }
                    query = "SELECT UserID, FirstName, LastName, NumberOfPassedTests, HighestScore, LowestScore " +
                             "FROM Users " +
                             "ORDER BY HighestScore DESC;";
                    using (SqlCommand retrieveCommand = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = retrieveCommand.ExecuteReader())
                        {
                            // do i need this?
                            users.Clear();
                            while (reader.Read())
                            {
                                int userID = reader.GetInt32(0);
                                string firstNameValue = reader.GetString(1);
                                string lastNameValue = reader.GetString(2);
                                int numberOfPassedTests = reader.GetInt32(3);
                                int highestScore = reader.GetInt32(4);
                                int lowestScore = reader.GetInt32(5);
                                User user = new User
                                {
                                    UserID = userID,
                                    FirstName = firstNameValue,
                                    LastName = lastNameValue,
                                    NumberOfPassedTests = numberOfPassedTests,
                                    HighestScore = highestScore,
                                    LowestScore = lowestScore
                                };
                                users.Add(user);
                            }
                            reader.Close();
                        }
                    }
                }
                connection.Close();
            }
            dgvLeaderboard.AutoGenerateColumns = true;
            dgvLeaderboard.DataSource = users;
            dgvLeaderboard.Columns[0].Width = 50;
        }
        private void btnRestart_Click(object sender, EventArgs e)
        {
            //show reg form
            UserForm uf = new UserForm();
            uf.Show();
            foreach (Form form in Application.OpenForms.Cast<Form>().ToList())
            {
                if (form != uf)
                {
                    form.Close();
                }
            }
        }
        private void btnExit_Click(object sender, EventArgs e)
        {
            foreach (Form form in Application.OpenForms.Cast<Form>().ToList())
            {
                    form.Close();   
            }
        }
    }
}
