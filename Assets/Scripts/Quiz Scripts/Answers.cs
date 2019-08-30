using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace MtsuMLAR
{/// <summary>
/// This script sends the user's quiz answers to a Google spreadsheet
/// </summary>
    public class Answers : MonoBehaviour
    {
        public int correct = 0;
        public int incorrect = 0;
        public int mlNumber;
        public float grade;
        public int total;

        private string totalString;
        private string gradeString;
        private string correctString;
        private string incorrectString;
        private string mlString;

        private string BASE_URL = "https://docs.google.com/forms/d/e/1FAIpQLSc8VT7Bf6RqZKWM2kQqEEwn1Y-h_8I27YH0onfl9ciWkPSG8A/formResponse"; // the Google Forms URL to send the answers to

        public void SendAnswers(int correct, int incorrect) // this function is called in Box_Controller. It sets the number of correct and incorrect answers
        {
            total = correct + incorrect;
            grade = correct / total;

            // convert to string to send to the form
            totalString = total.ToString();
            gradeString = grade.ToString();
            correctString = correct.ToString();
            incorrectString = incorrect.ToString();

            mlString = mlNumber.ToString();

            StartCoroutine(Post(mlString, gradeString, correctString, incorrectString));
        }

        IEnumerator Post(string id, string grade, string correct, string incorrect) // this enumerator sends data to Google Forms using Unity Web Request
        {
            WWWForm form = new WWWForm();
            form.AddField("entry.879842334", id);
            form.AddField("entry.1612244359", correct);
            form.AddField("entry.614974522", incorrect);
            form.AddField("entry.1601005384", grade);
            UnityWebRequest www = UnityWebRequest.Post(BASE_URL, form);
            yield return www.SendWebRequest();
        }
    }
}

