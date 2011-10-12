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
using osu_common.Helpers;

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
                Invoke((MethodInvoker)delegate
                {
                    checkBoxQuick.Checked = true;
                    CombinateAndTest(false);
                });
            }
        }

        int beatmapLength;

        private void visualisePackage(string package)
        {
            using (Beatmap b = new Beatmap(package))
            using (HitObjectManagerLoadAll hom = new HitObjectManagerLoadAll(b))
            {
                labelBeatmapTitle.Text = b.Title;
                labelBeatmapArtist.Text = b.Artist;

                hom.LoadFile();

                int width = beatmapLayout.Width;
                int height = beatmapLayout.Height;

                beatmapLayout.Image = new Bitmap(width, height);

                beatmapLength = hom.StreamHitObjects[3].FindLast(s => s == s).EndTime + 1000;

                using (Graphics g = Graphics.FromImage(beatmapLayout.Image))
                {
                    g.Clear(Color.White);

                    drawHitObjects(g, hom.StreamHitObjects[0], Color.YellowGreen, 0);
                    drawHitObjects(g, hom.StreamHitObjects[1], Color.CornflowerBlue, 1);
                    drawHitObjects(g, hom.StreamHitObjects[2], Color.PaleVioletRed, 2);
                    drawHitObjects(g, hom.StreamHitObjects[3], Color.BlueViolet, 3);

                    //draw bookmarks
                    if (b.StreamSwitchPoints != null)
                    {
                        foreach (int time in b.StreamSwitchPoints)
                        {
                            int xPos = (int)((float)time / beatmapLength * width);
                            g.DrawLine(new Pen(Color.Black), new Point(xPos - 1, 0), new Point(xPos - 1, height));
                            g.DrawLine(new Pen(Color.Red), new Point(xPos, 0), new Point(xPos, height));
                            g.DrawLine(new Pen(Color.Black), new Point(xPos + 1, 0), new Point(xPos + 1, height));
                        }
                    }
                }
            }
        }

        private void drawHitObjects(Graphics g, pList<HitObject> objects, Color color, int vOffset)
        {
            if (objects == null) return;

            int width = beatmapLayout.Width;
            int height = (int)(beatmapLayout.Height / 4f);

            int h1 = (int)(height * vOffset);
            int h2 = (int)(height * (vOffset + 1)) - 1;

            Pen brush = new Pen(Color.FromArgb(100, color.R, color.G, color.B));

            foreach (HitObject h in objects)
            {
                int objWidth = (int)Math.Max(1, (float)(h.EndTime - h.StartTime) / beatmapLength);
                g.DrawRectangle(brush, new Rectangle((int)((float)h.StartTime / beatmapLength * width), h1, objWidth, height));
            }
        }


        bool isDragging;
        private void beatmapLayout_MouseDown(object sender, MouseEventArgs e)
        {
            checkBoxEditorTime.Checked = false;

            updatePosition();
            isDragging = true;
        }

        private void beatmapLayout_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }

        private void updatePosition()
        {
            int time = (int)((float)beatmapLength * beatmapLayout.PointToClient(Cursor.Position).X / beatmapLayout.Width);
            updateStartTime(time);
        }

        private void updateStartTime(int time)
        {
            if (beatmapLength == 0 || time < 0 || time > beatmapLength)
                return;

            textBoxStartTime.Text = time.ToString();
            arrow.Location = new Point(this.PointToClient(Cursor.Position).X - arrow.Width / 2, arrow.Location.Y);
        }


        private void beatmapLayout_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
                updatePosition();
        }

        public Form1()
        {
            InitializeComponent();
            new TextBoxStreamWriter(console);
        }

        private void buttonTestOnce_Click(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(w => { CombinateAndTest(); });
        }

        private void button1_Click(object s1, EventArgs e1)
        {
            checkingForChanges = !checkingForChanges;
            if (checkingForChanges)
            {
                checkBoxEditorTime.Checked = true;
                buttonTestOnSave.Text = "Cancel";

                bool hasChanges = false;
                ThreadPool.QueueUserWorkItem(w =>
                {

                    FileSystemWatcher fsw = new FileSystemWatcher(filename);

                    string changedFilename = null;

                    fsw.Changed += delegate(object sender, FileSystemEventArgs e)
                    {
                        changedFilename = e.FullPath;
                        hasChanges = true;
                    };
                    fsw.EnableRaisingEvents = true;

                    Console.WriteLine("Waiting for changes...");

                    while (checkingForChanges)
                    {
                        if (hasChanges)
                        {
                            fsw.EnableRaisingEvents = false;

                            hasChanges = false;
                            Console.WriteLine("Detected changes; recombinating!");


                            if (checkBoxEditorTime.Checked)
                            {
                                foreach (string l in File.ReadAllLines(changedFilename))
                                {
                                    if (l.StartsWith("CurrentTime:"))
                                    {
                                        updateStartTime(Int32.Parse(l.Split(':')[1].Trim()));
                                        break;
                                    }
                                }
                            }

                            if (checkBoxEditorDifficulty.Checked)
                            {
                                if (changedFilename.Contains("Easy"))
                                    radioButtonEasy.Checked = true;
                                else if (changedFilename.Contains("Normal"))
                                    radioButtonNormal.Checked = true;
                                else if (changedFilename.Contains("Hard"))
                                    radioButtonHard.Checked = true;
                                else if (changedFilename.Contains("Expert"))
                                    radioButtonExpert.Checked = true;
                            }


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

        private string CombinateAndTest(bool runTest = true)
        {
            string packageName = null;

            Invoke((MethodInvoker)delegate
            {
                console.Text = string.Empty;
                panelButtons.Enabled = false;
            });

            if (game != null)
            {
                GameBase.Scheduler.Add(delegate
                {
                    Director.ChangeMode(OsuMode.PositioningTest, null);
                }, true);

                while (Director.CurrentOsuMode == OsuMode.PlayTest)
                    Thread.Sleep(50);
            }

            try
            {
                GameBase.Instance = null;
                //temporarily remove any instance to ensure we get correct and speedy calculations.
                //this will be restored at the end of processing.

                packageName = BeatmapCombinator.Process(Filename, checkBoxQuick.Checked, checkBoxm4a.Checked);

                GameBase.Instance = game;

                PlayTest.StartTime = Int32.Parse(textBoxStartTime.Text);
                PlayTest.AllowStreamSwitch = streamSwitch.Checked;
                Player.Beatmap = new Beatmap(packageName);
                Player.Autoplay = checkBoxAutoplay.Checked;

                if (radioButtonStreamUp.Checked)
                    PlayTest.InitialHp = 200;
                else if (radioButtonStreamDown.Checked)
                    PlayTest.InitialHp = 0;
                else
                    PlayTest.InitialHp = 100;

                if (radioButtonEasy.Checked)
                    PlayTest.InitialDifficulty = Difficulty.Easy;
                else if (radioButtonNormal.Checked)
                    PlayTest.InitialDifficulty = Difficulty.Normal;
                else if (radioButtonHard.Checked)
                    PlayTest.InitialDifficulty = Difficulty.Hard;
                else if (radioButtonExpert.Checked)
                    PlayTest.InitialDifficulty = Difficulty.Expert;

                switch (PlayTest.InitialDifficulty)
                {
                    case Difficulty.Expert:
                        Player.Difficulty = Difficulty.Expert;
                        break;
                    default:
                        Player.Difficulty = PlayTest.AllowStreamSwitch ? Difficulty.Normal : PlayTest.InitialDifficulty;
                        break;
                }

                Invoke((MethodInvoker)delegate { visualisePackage(packageName); });

                if (runTest && !checkBoxm4a.Checked)
                {
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
                        GameBase.Scheduler.Add(delegate { Director.ChangeMode(OsuMode.PlayTest, null); }, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR:\n" + ex.ToString());
            }

            Invoke((MethodInvoker)delegate { panelButtons.Enabled = true; });

            return packageName;
        }

        private void panel1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void panel1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Filename = files[0];
            panelDrop.BackColor = Color.DimGray;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Environment.Exit(-1);

            base.OnClosing(e);
        }

        private void streamSwitch_CheckedChanged(object sender, EventArgs e)
        {
            groupBoxStreamSwitch.Enabled = streamSwitch.Checked;
        }

        private void difficultyChanged(object sender, EventArgs e)
        {
            radioButtonStreamNormal.Checked = true;

            radioButtonStreamDown.Enabled = sender != radioButtonEasy && sender != radioButtonExpert;
            radioButtonStreamUp.Enabled = sender != radioButtonHard && sender != radioButtonExpert;

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            buttonTestOnSave.Enabled = !checkBoxm4a.Checked;
            groupBoxDifficulty.Enabled = !checkBoxm4a.Checked;
            groupBoxStreamSwitch.Enabled = !checkBoxm4a.Checked;

            if (checkBoxm4a.Checked)
            {
                checkBoxQuick.Enabled = false;
                checkBoxQuick.Checked = false;
            }
            else
            {
                checkBoxQuick.Enabled = true;
            }

            buttonTestOnce.Text = checkBoxm4a.Checked ? "Create Package" : "Test Once";
        }

    }

    public class HitObjectManagerLoadAll : HitObjectManager
    {
        public HitObjectManagerLoadAll(Beatmap beatmap)
            : base(beatmap)
        {

        }
        protected override bool shouldLoadDifficulty(Difficulty difficulty)
        {
            return true;
        }
    }
}
