using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class book : MonoBehaviour
{
    [SerializeField] private TextAsset _book;

    string paragraph = "";

    private void Awake()
    {
        print("start");
    }

    // Start is called before the first frame update
    void Start()
    { 
        string currnet_word = "";
        foreach (var letter in _book.text)
        {
            if (letter == '\n' || letter == ' ')
            {
                word_made(currnet_word);
                currnet_word = "";
            }
            else currnet_word += letter;

            paragraph += letter;
        }
        
    }
    void word_made(string word)
    {
        if (word.Length > 1)
        {
            if (word == "Тема:")
            {
                paragraph = "";
            }
            else if (word == "План:")
            {
                print(paragraph.Substring(0, paragraph.Length - 6));
                paragraph = "";
            }
        }
    }
}
