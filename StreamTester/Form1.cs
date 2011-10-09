using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using osum;
using osum.GameModes.Play;
using osum.GameModes;
using osum.GameplayElements.Beatmaps;
using System.IO;
using ConsoleRedirection;
using osum.GameplayElements;

namespace StreamTester
{
    public partial class Form1 : Form
    {
        string filename;
        private bool checkingForChanges;
        private GameBaseDesktop game;
        private string Filename
        {
            get { return filename; }
            set
            {
                filename = value;

                buttonTestOnce.Enabled = !File.Exists(filename);
                buttonTestOnSave.Enabled = !File.Exists(filename);
            }
        }

        public Form1()
        {
            InitializeComponent();
            new TextBoxStreamWriter(console);
        }

        private void buttonTestOnce_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(w =>
            {
                CombinateAndTest();
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            checkingForChanges = !checkingForChanges;
            if (checkingForChanges)
            {
                buttonTestOnSave.Text = "Cancel";

                bool hasChanges = false;
                ThreadPool.QueueUserWorkItem(w =>
                {

                    FileSystemWatcher fsw = new FileSystemWatcher(filename);
                    fsw.Changed += delegate { hasChanges = true; };
                    fsw.EnableRaisingEvents = true;

                    Console.WriteLine("Waiting for changes...");

                    while (checkingForChanges)
                    {
                        if (hasChanges)
                        {
                            fsw.EnableRaisingEvents = false;

                            hasChanges = false;
                            Console.WriteLine("Detected changes; recombinating!");

                            CombinateAndTest();

                            fsw.EnableRaisingEvents = true;
                            Console.WriteLine("Waiting for changes...");
                        }

                        Thread.Sleep(100);
                    }
                });
            }
            else
            {
                buttonTestOnSave.Text = "Automatically test on beatmap save";
            }

        }

        private void CombinateAndTest()
        {
            Invoke((MethodInvoker)delegate { console.Text = string.Empty; });

            if (game != null)
            {
                GameBase.Scheduler.Add(delegate
                {
                    Director.ChangeMode(OsuMode.PositioningTest, null);
                });

                while (Director.CurrentOsuMode == OsuMode.PlayTest)
                    Thread.Sleep(50);
            }

            try
            {
                string package = BeatmapCombinator.Process(Filename, checkBoxQuick.Checked);

                PlayTest.StartTime = Int32.Parse(textBoxStartTime.Text);
                PlayTest.AllowStreamSwitch = streamSwitch.Checked;
                Player.Beatmap = new Beatmap(package);
                Player.Autoplay = checkBoxAutoplay.Checked;

                if (radioButtonEasy.Checked)
                    Player.Difficulty = Difficulty.Easy;
                else if (radioButtonNormal.Checked)
                    Player.Difficulty = Difficulty.Normal;
                else if (radioButtonHard.Checked)
                    Player.Difficulty = Difficulty.Hard;
                else if (radioButtonExpert.Checked)
                    Player.Difficulty = Difficulty.Expert;

                if (game == null)
                {
                    ThreadPool.QueueUserWorkItem(w =>
                    {

                        game = new GameBaseDesktop(OsuMode.PlayTest);
                        game.Run();
                    });
                }
                else
                {
                    Director.ChangeMode(OsuMode.PlayTest);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:\n" + ex.ToString());
            }
        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Filename = files[0];
            beatmapName.Text = Path.GetFileName(Filename);

            panelDrop.BackColor = Color.DimGray;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Environment.Exit(-1);

            base.OnClosing(e);
        }
    }
}
