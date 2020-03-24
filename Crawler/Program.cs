using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            //requesting input for top word quantity
            Console.WriteLine("Enter top:");
            string top = Console.ReadLine();
            //accepting only numers greater than 0
            while (!Regex.IsMatch(top, @"^[0-9]+$") || Convert.ToInt32(top) < 1)
            {
                Console.WriteLine("You need to enter a number greater than 0. Please, try again:");
                top = Console.ReadLine();
            }

            //requesting words which user whant to exclude. You need to enter word and press enter. If you want
            //to proceed and see the words - enter empty string. Entered words will be excluded regardless of case
            List<string> exclude = new List<string>();
            string input = "s";
            while(input != "")
            {
                Console.WriteLine("Enter a word which you want to exclude or just press enter to proceed");
                input = Console.ReadLine();
                if (input != "")
                {
                    exclude.Add(input);
                }
            }

            //clearing the console for input and displaying information about user input
            Console.Clear();
            Console.WriteLine("Top " + top + " words displayed");
            Console.Write("Excluded words: ");
            foreach(var word in exclude)
            {
                Console.Write('\"' + word + "\" ");
            }
            Console.WriteLine("\nProcessing...");
            Console.WriteLine("------------------------------");

            //calling the crawler method which will process the web page and display the words.
            Crawl(Convert.ToInt32(top), exclude);
            Console.ReadLine();
        }

        private static async Task Crawl(int top, List<string> exclude)
        {
            //provided url string
            var url = "https://en.wikipedia.org/wiki/Microsoft";
            //instance of HttpClient
            var http = new HttpClient();
            //requesting html string
            var html = await http.GetStringAsync(url);
            //instance of html document and loading it from the string 
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            //sections of this page start and end with <h2> tag. So every h2 is a beginning of a new section
            var sections = htmlDoc.DocumentNode.Descendants("h2").ToList();
            //hashtable for words. The key is the word and the value is the how many times has it met
            Hashtable ht = new Hashtable();

            //loop for sections
            for (int i=0; i < sections.Count-1; i++)
            {
                //we look for <h2> with the <span> inside which has id = "History"
                if (sections[i].Descendants().FirstOrDefault().Id == "History")
                {
                    HtmlNode temp = sections[i];
                    //We are iterating through html elements with help of .NextSibling method
                    //which gets the html node element next to current element.
                    //We pass the text of each node to a function where it is splitted into a words and added to the hash table 
                    //we are doing it until we encounter node of next section - sections[i+1]. And then we break the loop
                    while (temp != sections[i+1])
                    {
                        splitAndAddToHash(temp.InnerText, ht, exclude);
                        temp = temp.NextSibling;
                    }
                    break;
                }
            }
            //order and display
            List<DictionaryEntry> list = ht.Cast<DictionaryEntry>().OrderByDescending(entry => entry.Value).ToList();
            for (int i = 0; i < top && i < list.Count; i++)
            {
                Console.WriteLine(list[i].Key.ToString() + " : " + list[i].Value.ToString());
            }
        }

        private static void splitAndAddToHash(string stringToSplit, Hashtable ht, List<string> exclude)
        {
            //splitting words with characters that we dont need in words. It will make some empty words when 
            //a character is in the end of a word and some "s" words when the given word is like "Microsoft's"
            //but it will all be handled
            string[] words = stringToSplit.Split(new char[] { ',', '\n', ' ', '.', '(', ')', ';', ':', '\'', '"', '?', '!', '*' });
            //loop to itearte through words
            foreach(string word in words)
            {
                
                //check if a word is not empty, contain only english letters of lowercase and uppercase and word is not "s" - there is no such a word.
                if (word != "" && Regex.IsMatch(word, @"^[a-zA-Z]+$") && word != "s")
                {
                    string upperWord = char.ToUpper(word[0]) + word.Substring(1); //word with first letter uppercase
                    string lowerWord = word.ToLower(); //word with all letters lowercase
                    //check if a word in any case not in the list of excluded words
                    if (!exclude.Contains(upperWord) && !exclude.Contains(lowerWord))
                    {
                        //if a key equal to lowercase word is already in the hash table - increment value
                        if (ht.Contains(lowerWord))
                        {
                            ht[lowerWord] = (int)ht[lowerWord] + 1;
                        }
                        else
                        {
                            //if a key of lowercase is not in a hash table, check word with first letter uppercase
                            if (ht.Contains(upperWord))
                            {
                                ht[upperWord] = (int)ht[upperWord] + 1;
                            }
                            //if no lowercase or uppercase word in the hash table then add it and set value to one
                            else
                            {
                                ht.Add(word, 1);
                            }
                        }
                    }

                }
            }
        }
    }
}
