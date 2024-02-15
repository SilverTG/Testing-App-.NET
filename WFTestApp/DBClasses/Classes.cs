using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClasses
{
    public class User
    {
        public User(){}
        public User(int userId,string firstName,string lastName,int numberOfPassedTests,int highestScore,int lowestScore)
        {
            UserID = userId;
            FirstName = firstName;
            LastName = lastName;
            NumberOfPassedTests = numberOfPassedTests;
            HighestScore = highestScore;
            LowestScore = lowestScore;
        }
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int NumberOfPassedTests { get; set; }
        public int HighestScore { get; set; }
        public int LowestScore { get; set; }
        public override string ToString() { return $"{UserID}|{FirstName}|{LastName}|{NumberOfPassedTests}|{HighestScore}|{LowestScore}"; }
        public static User FromString(string userString)
        {
            string[] values = userString.Split('|');

            if (values.Length != 6)
            {
                throw new ArgumentException("Invalid string format. Unable to parse User.");
            }

            User user = new User();
            user.UserID = int.Parse(values[0]);
            user.FirstName = values[1];
            user.LastName = values[2];
            user.NumberOfPassedTests = int.Parse(values[3]);
            user.HighestScore = int.Parse(values[4]);
            user.LowestScore = int.Parse(values[5]);

            return user;
        }
    }
    public class Question
    {
        public Question(){}
        public Question(int questionId,string questionText,string imageLink)
        {
            QuestionID = questionId;
            QuestionText = questionText;
            ImageLink = imageLink;
        }
        public int QuestionID { get; set; }
        public string QuestionText { get; set; }
        public string ImageLink { get; set; }
        public override string ToString() { return $"{QuestionID}|{QuestionText}|{ImageLink}"; }
        public static Question FromString(string questionString)
        {
            string[] values = questionString.Split('|');

            if (values.Length != 3)
            {
                throw new ArgumentException("Invalid string format. Unable to parse Question.");
            }

            Question question = new Question();
            question.QuestionID = int.Parse(values[0]);
            question.QuestionText = values[1];
            question.ImageLink = values[2];

            return question;
        }
    }
    public class Answer
    {
        public Answer(){}
        public Answer(int answerId,int questionId,string answerText,bool isCorrectAnswer)
        {
            AnswerID = answerId;
            QuestionID = questionId;
            AnswerText = answerText;
            IsCorrectAnswer = isCorrectAnswer;
        }
        public int AnswerID { get; set; }
        public int QuestionID { get; set; }
        public string AnswerText { get; set; }
        public bool IsCorrectAnswer { get; set; }
        public override string ToString() { return $"{AnswerID}|{QuestionID}|{AnswerText}|{IsCorrectAnswer}"; }
        public static Answer FromString(string answerString)
        {
            string[] values = answerString.Split('|');

            if (values.Length != 4)
            {
                throw new ArgumentException("Invalid string format. Unable to parse Answer.");
            }

            Answer answer = new Answer();
            answer.AnswerID = int.Parse(values[0]);
            answer.QuestionID = int.Parse(values[1]);
            answer.AnswerText = values[2];
            answer.IsCorrectAnswer = bool.Parse(values[3]);

            return answer;
        }

    }
}
