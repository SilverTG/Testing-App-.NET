using DBClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace WFTestAppAdmin
{
    public partial class TestingForm : Form
    {
        private List<Question> questions;
        private List<Answer> answers;
        private int time;
        private string lastName;
        private string firstName;
        private int questionNum = 0;
        private int points = 0;
        private int maxPoints = 0;
        private List<Answer> currentAnswers = new List<Answer>();
        public TestingForm(List<Question> q,List<Answer> a,int t,string fn,string ln)
        {
            questions = q;
            answers = a;
            lastName = ln;
            firstName = fn;
            time = t;

            InitializeComponent();
            lbTimer.Text = time.ToString();
            maxPoints = MaxPoints();
            //setting header
            this.Text = lastName + " " + firstName;
            //seting timer
            Task.Run(() => CountdownTimer(time)); // Run the countdown timer asynchronously in a separate thread
            RandomizeAnswers();
        }

        private void CountdownTimer(int minutes)
        {
            int remainingTime = minutes * 60;

            while (remainingTime > 0)
            {
                Thread.Sleep(1000); 
                remainingTime--;
                try
                {
                    BeginInvoke(new Action(() =>
                    {
                        TimeSpan timeSpan = TimeSpan.FromSeconds(remainingTime);
                        lbTimer.Text = timeSpan.ToString(@"mm\:ss");
                    }));
                }
                catch { }
            }
                BeginInvoke(new Action(() => 
                {
                    MessageBox.Show("Times Up!");
                })); // Update the label using Invoke to ensure thread-safe UI updates
        }

        private void TestingForm_Load(object sender, EventArgs e)
        {
            ShowQueston();
        }
        private void RandomizeAnswers()
        {
            Random random = new Random();
            int n = answers.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                Answer value = answers[k];
                answers[k] = answers[n];
                answers[n] = value;
            }
        }
        private void ShowQueston() 
        {
            cbAnswer1.Checked = false;
            cbAnswer2.Checked = false;
            cbAnswer3.Checked = false;
            cbAnswer4.Checked = false;

            rbAnswer1.Checked = false;
            rbAnswer2.Checked = false;
            rbAnswer3.Checked = false;
            rbAnswer4.Checked = false;
            lQuestionNumber.Text = "Question #" + (questionNum + 1).ToString();
            lQuestion.Text = questions[questionNum].QuestionText;
            try
            {
                pbImage.Load(questions[questionNum].ImageLink);
            }
            catch
            {   //loading filler image 
                pbImage.Load("source.unsplash.com/red-letters-neon-light-49uySSA678U");
            }
            pbImage.SizeMode = PictureBoxSizeMode.StretchImage;
            //getting current questions
            currentAnswers = new List<Answer>();
            foreach (Answer a in answers) 
            {
                if (a.QuestionID == questions[questionNum].QuestionID) 
                {
                    currentAnswers.Add(a);
                }
            }
            //checking if there is more than 1 answer
            bool moreThanOneMatch = answers.Count(a => a.QuestionID == questions[questionNum].QuestionID && a.IsCorrectAnswer == true) > 1;
            if (moreThanOneMatch) 
            {
                //setting visible checkboxes and setting text
               cbAnswer1.Visible= true;
               cbAnswer2.Visible= true;
               cbAnswer3.Visible= true;
               cbAnswer4.Visible= true;
                cbAnswer1.Text = currentAnswers[0].AnswerText;
                cbAnswer2.Text = currentAnswers[1].AnswerText;
                cbAnswer3.Text = currentAnswers[2].AnswerText;
                cbAnswer4.Text = currentAnswers[3].AnswerText;

                rbAnswer1.Visible = false;
                rbAnswer2.Visible = false;
                rbAnswer3.Visible = false;
                rbAnswer4.Visible = false;
            }
            else
            { //setting visible radioButtons and setting text
                rbAnswer1.Visible = true;
                rbAnswer2.Visible = true;
                rbAnswer3.Visible = true;
                rbAnswer4.Visible = true;
                rbAnswer1.Text = currentAnswers[0].AnswerText;
                rbAnswer2.Text = currentAnswers[1].AnswerText;
                rbAnswer3.Text = currentAnswers[2].AnswerText;
                rbAnswer4.Text = currentAnswers[3].AnswerText;

                cbAnswer1.Visible = false;
                cbAnswer2.Visible = false;
                cbAnswer3.Visible = false;
                cbAnswer4.Visible = false;
            }
            questionNum++;
        }
        private void CheckAnswers()
        {
            if (cbAnswer1.Visible)
            {
                if (cbAnswer1.Checked && currentAnswers[0].IsCorrectAnswer) points++;
                else if (cbAnswer1.Checked && !currentAnswers[0].IsCorrectAnswer) points--;
                if (cbAnswer2.Checked && currentAnswers[1].IsCorrectAnswer) points++;
                else if (cbAnswer2.Checked && !currentAnswers[1].IsCorrectAnswer) points--;
                if (cbAnswer3.Checked && currentAnswers[2].IsCorrectAnswer) points++;
                else if (cbAnswer3.Checked && !currentAnswers[2].IsCorrectAnswer)points--;
                if (cbAnswer4.Checked && currentAnswers[3].IsCorrectAnswer) points++;
                else if (cbAnswer4.Checked && !currentAnswers[3].IsCorrectAnswer) points--;
            }
            else 
            {
                if (rbAnswer1.Checked && currentAnswers[0].IsCorrectAnswer) points++;
                else if (rbAnswer2.Checked && currentAnswers[1].IsCorrectAnswer) points++;
                else if (rbAnswer3.Checked && currentAnswers[2].IsCorrectAnswer) points++;
                else if (rbAnswer4.Checked && currentAnswers[3].IsCorrectAnswer) points++;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            CheckAnswers();
            if (questionNum < questions.Count) ShowQueston();
            else 
            {
                TimeSpan timeSpan = TimeSpan.ParseExact(lbTimer.Text, @"mm\:ss", CultureInfo.InvariantCulture);
                int testTime = (time*60) - (int)timeSpan.TotalSeconds;
                //show result form and send data to admin
                ResultForm rf = new ResultForm(firstName,lastName,points, testTime);// SEND TIME HOW MUCH TIME WASTED ON TEST(START TIME - LEFT TIME)
                rf.Show();
                this.Close();
            }
        }

        private int MaxPoints() 
        {
            int mp = 0;
            foreach (Answer a in answers) 
            {
                if(a.IsCorrectAnswer) mp++;
            }
            return mp;
        }
    }
}
