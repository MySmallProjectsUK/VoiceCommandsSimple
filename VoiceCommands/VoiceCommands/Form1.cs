using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Speech.Recognition;
using System.Threading;

namespace VoiceCommands
{
    public partial class Form1 : Form
    {
        private bool IsFormClosing = false;

        private string StartListeningText = "Jarvis";
        private string StopListeningText = "Cancel";
        private TimeSpan StopListeningTimeout = new TimeSpan(0,0, 5);
        protected DateTime ListeningStarted = new DateTime(1950, 1, 1);
        //protected bool Listening = false;
        public bool Listening { get {
                if (DateTime.Now.Subtract(ListeningStarted) > StopListeningTimeout)
                {
                    return false;
                }
                else
                    return true;
            } }



        public Form1()
        {
            InitializeComponent();
        }
            // Create a new SpeechRecognitionEngine instance.
            //SpeechRecognizer recognizer = new SpeechRecognizer();
            SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine();

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadGrammar();
            StartRecognition();
             

        }

        private void LoadGrammar() {


            // Start Stop Listening            
            Choices c_StartStop = new Choices();
            c_StartStop.Add(new string[] { StartListeningText, StopListeningText });
            GrammarBuilder gb_StartStop = new GrammarBuilder();
            gb_StartStop.Append(c_StartStop);
            Grammar g_StartStop = new Grammar(gb_StartStop);


            // Create a simple grammar that recognizes "red", "green", or "blue".
            Choices colors = new Choices();
            colors.Add(new string[] { "red", "green", "blue", "white", "exit" , "Light On" , "Light Off" });
            // Create a GrammarBuilder object and append the Choices object.
            GrammarBuilder gb = new GrammarBuilder();            
            gb.Append(colors);
            // Create the Grammar instance and load it into the speech recognition engine.
            Grammar g_Comamnds = new Grammar(gb);
                        


            // numbers
            Choices ch_Numbers = new Choices();
            for(int i = 1; i<= 30; i++)
                ch_Numbers.Add(i.ToString());
            //ch_Numbers.Add("1");
            //ch_Numbers.Add("2");
            //ch_Numbers.Add("3");
            //ch_Numbers.Add("4"); // Technically Add(new string[] { "4" });
            //ch_Numbers.Add("30");
            //ch_Numbers.Add("13");
            GrammarBuilder gb_WhatIsXplusY = new GrammarBuilder();
            gb_WhatIsXplusY.Append("What is");
            gb_WhatIsXplusY.Append(ch_Numbers);
            gb_WhatIsXplusY.Append("plus");
            gb_WhatIsXplusY.Append(ch_Numbers);
            Grammar g_WhatIsXplusY = new Grammar(gb_WhatIsXplusY);


            // Build a Grammar object from the XML grammar.
            string fname = Application.StartupPath +"\\MediaMenuGrammar.grxml";
            if (!System.IO.File.Exists(fname))
                MessageBox.Show("Grammar File not found?" + Environment.NewLine + fname);
            
            Grammar g_mediaMusic = new Grammar(fname);
            


            recognizer.LoadGrammarAsync(g_StartStop);
            recognizer.LoadGrammarAsync(g_Comamnds);
            recognizer.LoadGrammarAsync(g_WhatIsXplusY);
            recognizer.LoadGrammarAsync(g_mediaMusic);


        }
        private void StartedListening() {
            //chkListening.Checked = Listening;
            ListeningStarted = DateTime.Now;
            //chkListening.Checked = Listening;
            //textBox1.BackColor = Color.Green;
            lblStatusListening.BackColor = Color.Green;
            lblStatusListening.ForeColor = Color.White;
            lblStatusListening.Text = "Listening...";
        }         
        private void StoppedListening() {
            //chkListening.Checked = Listening;
            ListeningStarted = DateTime.Now.Subtract(new TimeSpan(0, 0, 30));
            textBox1.Text = "";
            //textBox1.BackColor = Color.White;
            lblStatusListening.BackColor = Color.White;
            lblStatusListening.ForeColor = Color.Black;
            lblStatusListening.Text = "Idle";
        }

        private void StartRecognition() {
            recognizer.SpeechDetected += new EventHandler<SpeechDetectedEventArgs>(recognizer_SpeechDetected);
            recognizer.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(recognizer_SpeechRecognitionRejected);
            recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
            recognizer.RecognizeCompleted += new EventHandler<RecognizeCompletedEventArgs>(recognizer_RecognizeCompleted);


            Thread t1 = new Thread(delegate ()
            {
                recognizer.SetInputToDefaultAudioDevice();
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
            });
            t1.Start();
        }


        private void recognizer_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            textBox1.Text = "Recognizing voice command...";
        }

        // Create a simple handler for the SpeechRecognized event.
        private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            float confidence = e.Result.Confidence;
            
            textBox1.Text = "confidence: "  + confidence + ", Recognised:" + e.Result.Text;
            if (confidence < 0.9)
                return;

            string CommandHeard = e.Result.Text.ToLower();
            if (CommandHeard == StopListeningText.ToLower())
            {
                //ListeningStarted = DateTime.Now.Subtract(new TimeSpan(0, 0, 30));
                //textBox1.Text = "";
                StoppedListening();
            }
            if (CommandHeard == StartListeningText.ToLower())
            {
                StartedListening();
                //ListeningStarted = DateTime.Now;                
                //chkListening.Checked = Listening;                
               


                // keep a thread going to update the UI
                Thread t1 = new Thread(delegate ()
                {
                    while (Listening)
                    {
                        Application.DoEvents();
                    }

                    if(!IsFormClosing)
                    this.Invoke((MethodInvoker)delegate ()
                    {
                        //chkListening.Checked = Listening;
                        //textBox1.Text = "";
                        StoppedListening();
                    });

                });
                t1.Start();

                return;
            }

            // if not listening, do not do anything
            if (!Listening)
                return;


            lblLastComand.Text = e.Result.Text;

            if (CommandHeard == "exit")
            {
                recognizer.Dispose();
                Application.Exit();
            }
            if (CommandHeard == "red")
            {
                textBox1.BackColor = Color.Red;
            }
            else if (CommandHeard == "white")
            {
                textBox1.BackColor = Color.Wheat;
            }
            else if (CommandHeard == "green")
            {
                textBox1.BackColor = Color.Green;
            }

            else if (CommandHeard == "light on")
            {
                string cmd = Application.StartupPath + "\\commands\\Plug1-On.vbs";
                if (System.IO.File.Exists(cmd))
                    System.Diagnostics.Process.Start(cmd);
                else
                    MessageBox.Show("Action file not found:" + cmd);

            }
            else if (CommandHeard == "light off")
            {
                string cmd = Application.StartupPath + "\\commands\\Plug1-Off.vbs";
                if (System.IO.File.Exists(cmd))
                    System.Diagnostics.Process.Start(cmd);
                else
                    MessageBox.Show("Action file not found:" + cmd);
            }


        }

        private void recognizer_SpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            textBox1.Text = "Failure.";
        }

        private void recognizer_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            recognizer.RecognizeAsync();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsFormClosing = true;
        }
    }
}
