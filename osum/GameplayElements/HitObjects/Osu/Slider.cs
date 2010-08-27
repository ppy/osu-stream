using System;
using System.Collections.Generic;
using osum.GameplayElements;
using OpenTK;
using osum.Helpers;
using Color = OpenTK.Graphics.Color4;
using osu.Graphics.Primitives;
using osum.Graphics.Sprites;

namespace osu.GameplayElements.HitObjects.Osu
{
    internal class Slider : HitObject
    {
        #region General

        internal Slider(Vector2 startPosition, int startTime, bool newCombo, HitObjectSoundType soundType,
                           CurveTypes curveType, int repeatCount, double sliderLength, List<Vector2> sliderPoints, List<HitObjectSoundType> soundTypes)
            : base(startPosition, startTime)
        {
            CurveType = curveType;
            spriteManager = hitObjectManager.spriteManager;
            StartTime = startTime;
            EndTime = startTime;
            Position = startPosition;
            SoundType = soundType;

            if (sliderPoints == null)
            {
                sliderCurvePoints = new List<Vector2>();
                sliderCurvePoints.Add(Position);
            }
            else
            {
                sliderCurvePoints = sliderPoints;
                if (sliderCurvePoints.Count > 0)
                {
                    if (sliderCurvePoints[0] != Position)
                        sliderCurvePoints.Insert(0, Position);
                }
                else
                    sliderCurvePoints.Add(Position);
            }

            sliderRepeatCount = Math.Max(1, repeatCount);

            unifiedSoundAddition = soundTypes == null || soundTypes.Count == 0;
            if (!unifiedSoundAddition)
                SoundTypeList = soundTypes;

            this.sliderLength = sliderLength;
            Drawable = true;

            sliderEndCircles = new List<HitCircleSliderEnd>();

            Type = HitObjectType.Slider;
            if (newCombo)
                Type |= HitObjectType.NewCombo;

            sliderStartCircle =
                new HitCircleSliderStart(new Vector2((int)startPosition.X, (int)startPosition.Y), startTime,
                                 newCombo,
                                 unifiedSoundAddition ? SoundType : SoundTypeList[0], comboOffset);
            Transformation t = new Transformation(TransformationType.Fade, 0, 0, startTime - 10, startTime - 10);

            sliderStartCircle.SpriteHitCircle1.Transformations.RemoveAt(0);
            sliderStartCircle.SpriteHitCircle1.Transformations.Insert(0, t);

            //The overlay doesn't need to be drawn in some cases (saving some fill rate and double opacity).
            if (!HitObjectManager.ShowOverlayAboveNumber || (BeatmapManager.Current.OverlayPosition == OverlayPosition.NoChange && SkinManager.IsDefault))
            {
                sliderStartCircle.SpriteHitCircle2.Transformations.RemoveAt(0);
                sliderStartCircle.SpriteHitCircle2.Transformations.Insert(0, t);
            }


            sliderFollower =
                new pAnimation(SkinManager.LoadAll("sliderfollowcircle"), FieldTypes.Gamefield512x384,
                               OriginTypes.Centre, ClockTypes.Audio, Position, 0.99f, false, Color.Transparent,
                               this);
            sliderFollower.SetFramerateFromSkin();

            pTexture[] sliderballtextures = SkinManager.LoadAll("sliderb", SkinSource.All, false);

            sliderBall =
                new pAnimation(sliderballtextures, FieldTypes.Gamefield512x384, OriginTypes.Centre,
                               ClockTypes.Audio, Position, 0.99f, false, Color.White, this);
            //if (Player.currentScore == null || !ModManager.CheckActive(Player.currentScore.enabledMods, Mods.Taiko))
            sliderBall.TrackRotation = true;

            GameBase.OnUnload += GameBase_OnUnload;
        }

        void GameBase_OnUnload(bool all)
        {
            if (sliderBody != null)
            {
                sliderBody.Dispose();
                sliderBody = null;
            }
        }

        internal List<HitCircleOsu> sliderAllCircles
        {
            get
            {
                List<HitCircleOsu> l = new List<HitCircleOsu>();
                l.Add(sliderStartCircle);
                sliderEndCircles.ForEach(l.Add);
                return l;
            }
        }

        internal override HitObject Clone()
        {
            List<HitObjectSoundType> stl = null;
            if (SoundTypeList != null)
            {
                stl = new List<HitObjectSoundType>();
                stl.AddRange(SoundTypeList);
            }

            List<Vector2> scp = new List<Vector2>();
            scp.AddRange(sliderCurvePoints);


            Slider s = new Slider(Position, StartTime, (Type & HitObjectType.NewCombo) > 0, SoundType, CurveType, sliderRepeatCount,
                                        sliderLength, scp, stl, hitObjectManager, ComboOffset);

            s.ComboNumber = ComboNumber;
            s.sliderRepeatPoints = new List<int>(sliderRepeatPoints);
            s.EndTime = EndTime;
            s.SetColour(Colour);
            s.Selected = Selected;
            s.ComboColourIndex = ComboColourIndex; // fix Undo issue?
            return s;
        }

        internal override void Dispose()
        {
            GameBase.OnUnload -= GameBase_OnUnload;

            if (sliderBody != null)
                sliderBody.Dispose();
            base.Dispose();
        }

        #endregion

        #region Drawing

        internal readonly pAnimation sliderBall;
        internal readonly List<HitCircleSliderEnd> sliderEndCircles;

        internal readonly pAnimation sliderFollower;
        protected SpriteManager spriteManager;
        internal CurveTypes CurveType;
        private bool fullyDrawn;

        protected bool newCache = true;
        // Calling Update() sets this to true, which basically regenerates all paths/sprites

        internal pSprite sliderBody;

        internal List<Vector2> sliderCurvePoints;
        internal List<Line> sliderCurveSmoothLines;
        internal double sliderLength;
        internal int sliderRepeatCount;

        internal HitCircleOsu sliderStartCircle;

        internal override Vector2 EndPosition { get; set; }

        internal override int ComboNumber
        {
            get { return sliderStartCircle.ComboNumber; }
            set { sliderStartCircle.ComboNumber = value; }
        }

        internal override bool IsVisible
        {
            get
            {
                return
                    AudioEngine.Time >= StartTime - HitObjectManager.PreEmpt &&
                    AudioEngine.Time <= EndTime + HitObjectManager.FadeOut;
            }
        }

        internal override void Update()
        {
            if (GameBase.Mode == OsuModes.Edit || newCache)
                UpdateCalculations(newCache);

            if (sliderScoreTimingPoints.Count > 0)
            {
                int dotsPerRepeat = sliderScoreTimingPoints.Count / sliderRepeatCount;
                sliderBall.Reverse = (lastTickSounded + dotsPerRepeat + 1) / dotsPerRepeat % 2 == 0;

                if ((lastTickSounded + dotsPerRepeat + 1) / dotsPerRepeat % 2 == 0 && SkinManager.Current.SliderBallFlip)
                    sliderBall.SpriteEffect = startSpriteEffect | SpriteEffects.FlipHorizontally;
                else
                    sliderBall.SpriteEffect = startSpriteEffect;

                //slider ball operations
                sliderBall.frameSkip = (float)(150 / Velocity);
            }

            int closeTick = -1;
            for (int i = 0; i < sliderScoreTimingPoints.Count - 1; i++)
            {
                if (sliderScoreTimingPoints[i] > AudioEngine.Time)
                    break;
                closeTick = i;
            }

            if ((AudioEngine.Time >= StartTime &&
                 AudioEngine.Time <= EndTime) && AudioEngine.AudioState == AudioStates.Playing
                // && !(GameBase.Mode == OsuModes.Edit && !HitObjectManager.AutoPlay)
                )
            {
                if (IsSliding)
                {
                    SliderSoundStart();

                    if (closeTick > lastTickSounded)
                    {
                        lastTickSounded = closeTick;

                        if ((lastTickSounded + 1) % (sliderScoreTimingPoints.Count / sliderRepeatCount) > 0)
                            PlaySound(true, lastTickSounded);
                        else
                            PlaySound(false, (lastTickSounded + 1) / (sliderScoreTimingPoints.Count / sliderRepeatCount));

                        pSprite scoreTick =
                            SpriteCollection.Find(p => p.TagNumeric == sliderScoreTimingPoints[lastTickSounded]);
                        if (scoreTick != null && scoreTick.Transformations.Count > 1)
                        {
                            scoreTick.Transformations[1].Time1 = sliderScoreTimingPoints[lastTickSounded];
                            scoreTick.Transformations[1].Time2 = sliderScoreTimingPoints[lastTickSounded];
                        }
                        lastTickSounded = closeTick;
                    }
                }
                else
                {
                    StopSound();
                    lastTickSounded = closeTick;
                }
            }
            else
            {
                //StopSound();
                lastTickSounded = -1;
            }
        }

        internal virtual void UpdateCalculations(bool force)
        {
            if (!(force || placingPending))
                return;

            RemoveBody();

            Velocity = HitObjectManager.SliderVelocityAt(StartTime);

            List<Line> path = new List<Line>();

            if (placingPending)
            {
                if (sliderCurvePoints.Contains(placingPoint))
                    placingPending = false;
                else
                    sliderCurvePoints.Add(placingPoint);
            }

            switch (CurveType)
            {
                case CurveTypes.Catmull:
                    //yuck. we don't need these do we?!

                    /*for (int j = 0; j < sliderCurvePoints.Count - 1; j++)
                    {
                        Vector2 v1 = (j - 1 >= 0 ? sliderCurvePoints[j - 1] : sliderCurvePoints[j]);
                        Vector2 v2 = sliderCurvePoints[j];
                        Vector2 v3 = (j + 1 < sliderCurvePoints.Count
                                          ? sliderCurvePoints[j + 1]
                                          : v2 + (v2 - v1));
                        Vector2 v4 = (j + 2 < sliderCurvePoints.Count
                                          ? sliderCurvePoints[j + 2]
                                          : v3 + (v3 - v2));

                        for (int k = 0; k < General.SLIDER_DETAIL_LEVEL; k++)
                            path.Add(
                                new Line(Vector2.CatmullRom(v1, v2, v3, v4, (float)k / General.SLIDER_DETAIL_LEVEL),
                                         Vector2.CatmullRom(v1, v2, v3, v4,
                                                            (float)(k + 1) / General.SLIDER_DETAIL_LEVEL)));
                    }*/
                    break;
                case CurveTypes.Bezier:
                    int lastIndex = 0;
                    for (int i = 0; i < sliderCurvePoints.Count; i++)
                    {
                        bool multipartSegment = i < sliderCurvePoints.Count - 2 && sliderCurvePoints[i] == sliderCurvePoints[i + 1];

                        if (multipartSegment || i == sliderCurvePoints.Count - 1)
                        {
                            List<Vector2> thisLength = sliderCurvePoints.GetRange(lastIndex, i - lastIndex + 1);

                            List<Vector2> points = pMathHelper.CreateBezier(thisLength);
                            for (int j = 1; j < points.Count; j++)
                                path.Add(new Line(points[j - 1], points[j]));

                            if (multipartSegment) i++;
                            //Need to skip one point since we consuned an extra.

                            lastIndex = i;
                        }
                    }
                    break;
                case CurveTypes.Linear:
                    for (int i = 1; i < sliderCurvePoints.Count; i++)
                    {
                        Line l = new Line(sliderCurvePoints[i - 1], sliderCurvePoints[i]);
                        int segments = Math.Max((int)l.rho / 10, 1);
                        for (int j = 0; j < segments; j++)
                            path.Add(
                                new Line(l.p1 + (l.p2 - l.p1) * ((float)j / segments),
                                         l.p1 + (l.p2 - l.p1) * ((float)(j + 1) / segments)));
                    }
                    break;
            }

            //if (GameBase.GameState == GameStates.Edit)
            //{
            double total = 0;

            int pathCount = path.Count;
            for (int i = 0; i < pathCount; i++)
                total += path[i].rho;

            int aimCount = 0;
            double tickDistance = (BeatmapManager.Current.BeatmapVersion < 8) ? HitObjectManager.SliderScoringPointDistance :
                (HitObjectManager.SliderScoringPointDistance / AudioEngine.bpmMultiplierAt(StartTime)); // Keep tick rate a TIME CONSTANT so non 0.5/2x slider speeds are well-behaved.

            //this will make sure the length of the slider is a multiple of the scoring points
            if (total > 0)
            {
                if (placingPending || sliderLength == 0)
                {
                    //Currently in editor mode
                    if (Editor.Instance != null && Editor.isBeatSnapping)
                    {
                        double f = (tickDistance * BeatmapManager.Current.DifficultySliderTickRate) / Editor.Instance.beatSnapDivisor;
                        aimCount = (int)Math.Max(0, (total + 2.0d) / f); // Slightly more generous rounding so we can have super symmetry sliders.
                        sliderLength = aimCount * f;
                    }
                    else
                    {
                        aimCount = (int)(total / tickDistance);
                        sliderLength = total;
                    }
                }

                if (tickDistance > sliderLength)
                    tickDistance = sliderLength;

                if (successfulPlace)
                {
                    successfulPlaceLength = aimCount;
                    successfulPlace = false;
                }

                double excess = total - sliderLength;

                while (path.Count > 0)
                {
                    Line lastLine = path[path.Count - 1];
                    float lastLineLength = Vector2.Distance(lastLine.p1, lastLine.p2);

                    if (lastLineLength > excess)
                    {
                        lastLine.p2 = lastLine.p1 +
                                      Vector2.Normalize(lastLine.p2 - lastLine.p1) * (lastLine.rho - (float)excess);
                        lastLine.Recalc();

                        break;
                    }

                    path.Remove(lastLine);
                    excess -= lastLineLength;
                }
            }

            if (placingPending)
            {
                if (aimCount == successfulPlaceLength && aimCount != 0)
                {
                    sliderCurvePoints.RemoveAt(sliderCurvePoints.Count - 1);
                    placingValid = false;
                }
                else
                    placingValid = true;

                placingPending = false;
            }


            pathCount = path.Count;

            if (pathCount > 0)
            {
                //fill the cache
                sliderCurveSmoothLines = path;
            }

            if (spriteManager != null)
            {
                if (SpriteCollection.Count > 0)
                    spriteManager.RemoveRange(SpriteCollection);

                //update the follower path
                sliderFollower.Transformations.Clear();
                sliderBall.Transformations.Clear();
                sliderScoreTimingPoints.Clear();
                sliderEndCircles.Clear();
            }

            if (pathCount < 1)
                return;

            if (spriteManager != null)
            {
                SpriteCollection.Clear();
                DimCollection.Clear();

                if (sliderBody != null)
                    SpriteCollection.Add(sliderBody);


                //add in the static sprites which won't be changed in this method
                for (int i = 0; i < sliderStartCircle.SpriteCollection.Count; i++)
                {
                    pSprite p = sliderStartCircle.SpriteCollection[i];
                    p.Tag = this;
                    SpriteCollection.Add(p);
                }

                for (int i = 0; i < sliderStartCircle.DimCollection.Count; i++)
                {
                    pSprite p = sliderStartCircle.DimCollection[i];
                    DimCollection.Add(p);
                }

                SpriteCollection.Add(sliderFollower);
                SpriteCollection.Add(sliderBall);
            }

            //draw circles and calculate the follow path
            bool reverse = false;
            bool firstRun = true;
            double scoringLengthTotal = 0;
            double currentTime = StartTime;
            Vector2 p2 = new Vector2();
            Vector2 p1 = new Vector2();

            double scoringDistance = 0;

            Position2 = path[pathCount - 1].p2;

            if (path.Count > 2)
            {
                float ballStartAngle = (float)Math.Atan2(path[0].p1.Y - path[0].p2.Y, path[0].p1.X - path[0].p2.X);
                startSpriteEffect = ballStartAngle >= -Math.PI / 2 && ballStartAngle <= Math.PI / 2 ? SpriteEffects.FlipVertically : SpriteEffects.None;
            }

            for (int i = 0; i < sliderRepeatCount; i++)
            {
                int reverseStartTime = (int)currentTime;

                bool endAccounted = false;

                List<pSprite> scoringDots = new List<pSprite>();

                for (int j = 0; j < pathCount; j++)
                {
                    Line l = path[j];

                    if (reverse)
                    {
                        p1 = l.p2;
                        p2 = l.p1;
                    }
                    else
                    {
                        p1 = l.p1;
                        p2 = l.p2;
                    }

                    //path for follower and ball
                    float distance = Vector2.Distance(l.p1, l.p2);

                    double duration = 1000F * distance / Velocity;

                    Transformation t =
                        new Transformation(p1, p2, (int)currentTime, (int)(currentTime + duration));
                    sliderFollower.Transformations.Add(t);
                    sliderBall.Transformations.Add(t);

                    currentTime += duration;
                    scoringDistance += distance;

                    //sprites for scoring points (dots)
                    if (scoringDistance >= tickDistance ||
                        ((j == pathCount - 1) && !endAccounted))
                    {
                        scoringLengthTotal +=
                            Math.Min(tickDistance, scoringDistance);

                        int scoreTime = timeAtLength((float)scoringLengthTotal);

                        if (j == pathCount - 1)
                        {
                            scoringLengthTotal -= (tickDistance - scoringDistance);
                            scoringDistance = tickDistance - scoringDistance;
                            endAccounted = true;
                        }
                        else
                            scoringDistance -= Math.Min(tickDistance, scoringDistance);

                        if (spriteManager != null)
                        {
                            if (j != pathCount - 1)
                            {


                                float thisPointRatio = 1 - (float)(scoringDistance / Vector2.Distance(p1, p2));
                                Vector2 adjustedPos = p1 + (p2 - p1) * (thisPointRatio);


                                pSprite scoringDot =
                                    new pSprite(SkinManager.Load("sliderscorepoint"),
                                                FieldTypes.Gamefield512x384, OriginTypes.Centre, ClockTypes.Audio, adjustedPos,
                                                SpriteManager.drawOrderBwd(StartTime - 5), false, Color.White, this);
                                //handle the first dots appearing on the preempt
                                if (firstRun)
                                {
                                    scoringDot.Transformations.Add(
                                        new Transformation(TransformationType.Fade, 0, 1,
                                                           StartTime - HitObjectManager.PreEmptSliderComplete,
                                                           StartTime - HitObjectManager.PreEmptSliderComplete +
                                                           HitObjectManager.FadeIn));
                                }
                                else
                                    scoringDot.Transformations.Add(
                                        new Transformation(TransformationType.Fade, 0, 1, reverseStartTime - 200,
                                                           reverseStartTime));
                                float radiusSquared = HitObjectManager.HitObjectRadius * HitObjectManager.HitObjectRadius;
                                if (GameBase.Mode != OsuModes.Play ||
                                    (Vector2.DistanceSquared(adjustedPos, Position2) >= radiusSquared &&
                                     Vector2.DistanceSquared(adjustedPos, Position) >= radiusSquared))
                                {
                                    SpriteCollection.Add(scoringDot);
                                    scoringDots.Add(scoringDot);
                                }

                                scoringDot.TagNumeric = scoreTime;
                            }
                        }

                        sliderScoreTimingPoints.Add(scoreTime);
                    }
                }

                if (spriteManager != null)
                    foreach (pSprite p in scoringDots)
                        p.Transformations.Add(
                            new Transformation(TransformationType.Fade, 1, 0, (int)currentTime, (int)currentTime));

                float angle = (float)Math.Atan2(p1.Y - p2.Y, p1.X - p2.X);

                reverse = !reverse;
                path.Reverse();

                if (spriteManager != null)
                {
                    HitCircleSliderEnd h =
                        new HitCircleSliderEnd(p2, (int)(currentTime),
                                               (firstRun
                                                    ? StartTime - HitObjectManager.PreEmptSliderComplete
                                                    : reverseStartTime - (int)(currentTime - reverseStartTime)),
                                               (i < sliderRepeatCount - 1), angle, this);
                    h.SetColour(Colour);
                    sliderEndCircles.Add(h);

                    if (firstRun)
                        DimCollection.AddRange(h.DimCollection);
                    SpriteCollection.AddRange(h.SpriteCollection);
                }
                firstRun = false;
            }

            if (reverse)
                path.Reverse();

            if (Selected && sliderEndCircles.Count > 0)
                sliderEndCircles[0].Select();

            EndPosition = p2;
            EndTime = (int)currentTime;

            //a little hack to make sure the last scoring point occurs before the slider is totally gone.
            //i REALLY doubt this will be noticeable in any game mode.
            //This is also inaudible.
            if (sliderScoreTimingPoints.Count > 0)
            {
                sliderScoreTimingPoints[sliderScoreTimingPoints.Count - 1] = Math.Max(StartTime + (EndTime - StartTime) / 2, sliderScoreTimingPoints[sliderScoreTimingPoints.Count - 1] - 36);
            }



            sliderRepeatPoints.Clear();
            for (int i = 0; i < sliderScoreTimingPoints.Count - 1; i++)
                if (sliderScoreTimingPoints.Count / sliderRepeatCount > 0 &&
                    (i + 1) % (sliderScoreTimingPoints.Count / sliderRepeatCount) == 0)
                    sliderRepeatPoints.Add(sliderScoreTimingPoints[i]);

            //update sliding process
            if (IsSliding)
                KillSlide();

            if (spriteManager != null)
                spriteManager.Add(SpriteCollection);

            newCache = false;
        }

        internal override void Draw()
        {
            if (IsVisible && sliderCurveSmoothLines != null)
            {
                if ((!fullyDrawn && GameBase.SixtyFramesPerSecondFrame) || sliderBody == null ||
                    sliderBody.Texture == null || sliderBody.Texture.IsDisposed)
                {
                    RemoveBody();

                    List<Line> lineList = new List<Line>();

                    bool DisableColourRotation;

                    Color sliderColour = ((DisableColourRotation = SkinManager.LoadColour("SliderTrackOverride").A > 0)
                                              ? SkinManager.LoadColour("SliderTrackOverride")
                                              : new Color(Colour.R, Colour.G, Colour.B,
                                                          (byte)Math.Max(0, (int)Colour.A)));

                    int storedStart = 0;
                    bool waiting = false;

                    float progress = (GameBase.Mode == OsuModes.Edit || !ConfigManager.sSnakingSliders)
                                         ? 1
                                         : Math.Min(1,
                                                    (AudioEngine.Time - (StartTime - HitObjectManager.PreEmpt)) /
                                                    (HitObjectManager.PreEmpt / 3f));

                    if (progress == 1)
                        fullyDrawn = true;


                    int count = (int)(sliderCurveSmoothLines.Count * progress);
                    float countRemainder = sliderCurveSmoothLines.Count * progress - count;

                    count = Math.Max(count, 1);

                    Vector2 pos1 = GameBase.GamefieldToDisplay(Position);
                    Vector2 pos2 = GameBase.GamefieldToDisplay(Position2);

                    for (int i = 0; i < count; i++)
                    {
                        if (!waiting)
                            storedStart = i;

                        bool last = i == count - 1;

                        if (Vector2.Distance(sliderCurveSmoothLines[storedStart].p1, sliderCurveSmoothLines[i].p2) > 6 ||
                            last)
                        {
                            if (last && countRemainder > 0)
                            {
                                Line l = new Line(sliderCurveSmoothLines[storedStart].p1, sliderCurveSmoothLines[i].p2);
                                l.p2 = l.p1 + Vector2.Normalize(l.p2 - l.p1) * (l.rho * countRemainder);

                                GameBase.GamefieldToDisplay(ref l);
                                l.Recalc();

                                if (!fullyDrawn)
                                    pos2 = l.p2;

                                lineList.Add(l);
                            }
                            else if (storedStart == i)
                                lineList.Add(GameBase.GamefieldToDisplay(sliderCurveSmoothLines[i]));
                            else
                                lineList.Add(
                                    GameBase.GamefieldToDisplay(
                                        new Line(sliderCurveSmoothLines[storedStart].p1, sliderCurveSmoothLines[i].p2)));
                            waiting = false;
                        }
                        else
                            waiting = true;
                    }

                    if (count == 0)
                        return;

                    Vector2 centre = HitObjectManager.GamefieldSpriteRes / 2 * Vector2.One;

                    int width, height, left = 0, top = 0;

                    float radius = HitObjectManager.HitObjectRadius * GameBase.GamefieldRatio *
                                   (GameBase.D3D && GameBase.PixelShaderVersion < 2 ? (float)19 / 20 : 1);

                    int lineListCount = lineList.Count;

                    if (lineListCount > 0)
                    {
                        Vector2 topLeft = lineList[0].p1;
                        Vector2 bottomRight = lineList[0].p1;

                        for (int i = 0; i < lineListCount; i++)
                        {
                            Line l = lineList[i];
                            if (l.p2.X < topLeft.X)
                                topLeft.X = l.p2.X;
                            else if (l.p2.X > bottomRight.X)
                                bottomRight.X = l.p2.X;

                            if (l.p2.Y < topLeft.Y)
                                topLeft.Y = l.p2.Y;
                            else if (l.p2.Y > bottomRight.Y)
                                bottomRight.Y = l.p2.Y;
                        }

                        width = (int)(bottomRight.X - topLeft.X + radius * 2.3);
                        height = (int)(bottomRight.Y - topLeft.Y + radius * 2.3);
                        left = (int)(topLeft.X - radius * 1.15);
                        top = (int)(topLeft.Y - radius * 1.15);
                    }
                    else
                    {
                        width = (int)(radius * 2.2);
                        height = (int)(radius * 2.2);
                    }

                    int drawWidth = width;
                    int drawHeight = height;
                    int drawLeft = left;
                    int drawTop = top;


                    if (GameBase.Mode == OsuModes.Edit)
                    {
                        //make sure we are within bounds of the screen while drawing (otherwise things look ugly)
                        //only needs to run in edit mode, because in-game sliders are never moved back on-screen even if they are
                        //off-screen to begin with (which shouldn't happen anyway!).

                        int excess = 0;

                        excess = (drawTop + drawHeight) - (GameBase.WindowHeight + GameBase.WindowOffsetY);
                        if (excess > 0)
                        {
                            //goes off bottom of screen
                            for (int i = 0; i < lineListCount; i++)
                            {
                                Line l = lineList[i];
                                l.p1.Y -= excess;
                                l.p2.Y -= excess;
                            }

                            drawTop -= excess;
                            pos1.Y -= excess;
                            pos2.Y -= excess;
                        }

                        excess = (drawLeft + drawWidth) - (GameBase.WindowWidth);
                        if (excess > 0)
                        {
                            //goes off bottom of screen
                            for (int i = 0; i < lineListCount; i++)
                            {
                                Line l = lineList[i];
                                l.p1.X -= excess;
                                l.p2.X -= excess;
                            }

                            drawLeft -= excess;
                            pos1.X -= excess;
                            pos2.X -= excess;
                        }

                        excess = -drawTop;
                        if (excess > 0)
                        {
                            //goes off bottom of screen
                            for (int i = 0; i < lineListCount; i++)
                            {
                                Line l = lineList[i];
                                l.p1.Y += excess;
                                l.p2.Y += excess;
                            }

                            drawTop += excess;
                            pos1.Y += excess;
                            pos2.Y += excess;
                        }

                        excess = -drawLeft;
                        if (excess > 0)
                        {
                            //goes off bottom of screen
                            for (int i = 0; i < lineListCount; i++)
                            {
                                Line l = lineList[i];
                                l.p1.X += excess;
                                l.p2.X += excess;
                            }

                            drawLeft += excess;
                            pos1.X += excess;
                            pos2.X += excess;
                        }
                    }

                    //start the drawing procedure
                    if (GameBase.D3D)
                    {
                        // TODO: Hang onto this rendertarget across frames to increase snaking slider performance.
                        RenderTarget2D renderTarget =
                            new RenderTarget2D(GameBase.graphics.GraphicsDevice, GameBase.WindowWidth,
                                               GameBase.WindowHeight + GameBase.WindowOffsetY, 1, SurfaceFormat.Color);

                        GameBase.graphics.GraphicsDevice.SetRenderTarget(0, renderTarget);

                        GameBase.graphics.GraphicsDevice.Clear(Color.TransparentBlack);

                        switch (SkinManager.Current.SliderStyle)
                        {
                            case SliderStyle.PeppySliders:
                                GameBase.spriteBatch.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                                GameBase.graphics.GraphicsDevice.RenderState.SeparateAlphaBlendEnabled = true;
                                GameBase.spriteBatch.GraphicsDevice.RenderState.AlphaSourceBlend = Blend.One;
                                GameBase.spriteBatch.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.InverseSourceAlpha;

                                GameBase.LineManager.Draw(lineList, radius * 59 / 64,
                                                          Selected
                                                              ? (unifiedSoundAddition
                                                                     ? new Color(49, 151, 255)
                                                                     : new Color(229, 44, 44))
                                                              : SkinManager.LoadColour("SliderBorder"),
                                                          0, "StandardBorder", true);

                                GameBase.graphics.GraphicsDevice.RenderState.SeparateAlphaBlendEnabled = false;
                                GameBase.spriteBatch.GraphicsDevice.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
                                GameBase.spriteBatch.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.InverseSourceAlpha;

                                GameBase.LineManager.Draw(lineList,
                                                          HitObjectManager.HitObjectRadius * GameBase.GamefieldRatio * 52 / 64,
                                                          new Color(sliderColour.R, sliderColour.G, sliderColour.B, 255),
                                                          0, "Standard", true);

                                break;
                            case SliderStyle.MmSliders:
                            case SliderStyle.ToonSliders:
                                if (Selected)
                                    SkinManager.SliderManager.Draw(lineList, HitObjectManager.HitObjectRadius * GameBase.GamefieldRatio, sliderColour,
                                                                   unifiedSoundAddition
                                                                     ? new Color(49, 151, 255)
                                                                     : new Color(229, 44, 44), null);
                                else
                                {
                                    int index;
                                    if (this.CustomTagColor) index = -2; // Multiplay custom colour
                                    else if (this.IsGrey) index = -1; // Multiplay grey note
                                    else index = this.ComboColourIndex;

                                    SkinManager.SliderManager.Draw(lineList, HitObjectManager.HitObjectRadius * GameBase.GamefieldRatio, DisableColourRotation ? 0 : index, null);
                                }
                                break;
                        }


                        GameBase.spriteBatch.GraphicsDevice.RenderState.AlphaBlendEnable = true;
                        GameBase.graphics.GraphicsDevice.RenderState.SeparateAlphaBlendEnabled = false;
                        GameBase.spriteBatch.GraphicsDevice.RenderState.AlphaSourceBlend = Blend.SourceAlpha;
                        GameBase.spriteBatch.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.InverseSourceAlpha;

                        //Draw the end-points using a spriteBatch. // TODO: separate endpoint sprites instead of drawing to slider texture.
                        GameBase.spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Immediate,
                                                   SaveStateMode.None);

                        GameBase.spriteBatch.GraphicsDevice.RenderState.SeparateAlphaBlendEnabled = true;
                        GameBase.spriteBatch.GraphicsDevice.RenderState.AlphaSourceBlend = Blend.One;
                        GameBase.spriteBatch.GraphicsDevice.RenderState.AlphaDestinationBlend = Blend.InverseSourceAlpha;

                        GameBase.spriteBatch.Draw(SkinManager.Load("hitcircle").TextureXna, pos2, null, Colour, 0,
                                                  centre, HitObjectManager.SpriteRatio, SpriteEffects.None, 0.3f);
                        GameBase.spriteBatch.Draw(SkinManager.Load("hitcircleoverlay").TextureXna, pos2, null, // TODO: i92
                                                  Color.White, 0, centre, HitObjectManager.SpriteRatio,
                                                  SpriteEffects.None,
                                                  0.4f);
                        GameBase.spriteBatch.Draw(SkinManager.Load("hitcircle").TextureXna, pos1, null, Colour, 0,
                                                  centre, HitObjectManager.SpriteRatio, SpriteEffects.None, 0.5f);

                        GameBase.spriteBatch.Draw(SkinManager.Load("hitcircleoverlay").TextureXna, pos1, null, // TODO: i92
                                                  Color.White, 0, centre, HitObjectManager.SpriteRatio,
                                                  SpriteEffects.None,
                                                  0.6f);
                        GameBase.spriteBatch.End();

                        GameBase.graphics.GraphicsDevice.ResolveRenderTarget(0);

                        if (sliderBody == null)
                        {
                            sliderBody =
                                new pSprite(new pTexture(renderTarget.GetTexture()), FieldTypes.Native,
                                            OriginTypes.TopLeft,
                                            ClockTypes.Audio, new Vector2(left, top),
                                            SpriteManager.drawOrderBwd(EndTime + 10),
                                            false, Color.White);
                            sliderBody.Disposable = true;
                            SpriteCollection.Add(sliderBody);
                            spriteManager.Add(sliderBody);
                        }
                        else
                        {
                            sliderBody.Texture = new pTexture(renderTarget.GetTexture());
                            sliderBody.CurrentPosition = new Vector2(left, top);
                            sliderBody.Depth = SpriteManager.drawOrderBwd(EndTime + 10);
                        }

                        GameBase.graphics.GraphicsDevice.SetRenderTarget(0, null);

                        sliderBody.DrawLeft = drawLeft;
                        sliderBody.DrawTop = drawTop;
                        sliderBody.DrawWidth = drawWidth;
                        sliderBody.DrawHeight = drawHeight;
                    }
                    else
                    {
                        int xSize = width, ySize = height; //size of texture

                        if (GlControl.SurfaceType == Gl.GL_TEXTURE_2D)
                        {
                            xSize = TextureGl.GetPotDimension(xSize);
                            ySize = TextureGl.GetPotDimension(ySize);
                        }

                        Gl.glViewport(0, 0, xSize, ySize);

                        // Select The Projection Matrix
                        Gl.glMatrixMode(Gl.GL_PROJECTION);
                        // Reset The Projection Matrix
                        Gl.glLoadIdentity();
                        Gl.glOrtho(drawLeft, drawLeft + xSize, drawTop, drawTop + ySize, -1, 1);
                        // Select The Modelview Matrix
                        Gl.glMatrixMode(Gl.GL_MODELVIEW);
                        // Reset The Modelview Matrix
                        Gl.glLoadIdentity();

                        Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);
                        Gl.glClear(Gl.GL_DEPTH_BUFFER_BIT);

                        switch (SkinManager.Current.SliderStyle)
                        {
                            case SliderStyle.PeppySliders: // Hmm not sure what to do with these. They look similar to mmsliders but aliasy and with artifacts at turns.
                            /*    Color border;
                                if (Selected)
                                {
                                    border = unifiedSoundAddition ? new Color(49, 151, 255, 200) : new Color(229, 44, 44, 200);
                                }
                                else
                                {
                                    border = SkinManager.LoadColour("SliderBorder");
                                    border = new Color(border.R, border.G, border.B, 200);
                                }

                                Gl.glBlendFunc(Gl.GL_ONE, Gl.GL_ONE_MINUS_SRC_ALPHA);

                                GameBase.LineManager.Draw(lineList, radius * 58 / 64, border, 0, "StandardBorder", true);

                                Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);

                                GameBase.LineManager.Draw(lineList, radius * 51 / 64,
                                                          new Color(sliderColour.R, sliderColour.G, sliderColour.B, 255), 0,
                                                          "StandardNoRect", true);
                                break;*/
                            case SliderStyle.MmSliders:
                            case SliderStyle.ToonSliders:
                                if (Selected)
                                    SkinManager.SliderManager.Draw(lineList, HitObjectManager.HitObjectRadius * GameBase.GamefieldRatio, sliderColour,
                                                                   unifiedSoundAddition
                                                                     ? new Color(49, 151, 255)
                                                                     : new Color(229, 44, 44), null, new Rectangle(drawLeft, drawTop, xSize, ySize));
                                else
                                {
                                    int index;
                                    if (this.CustomTagColor) index = -2; // Multiplay custom colour
                                    else if (this.IsGrey) index = -1; // Multiplay grey note
                                    else index = this.ComboColourIndex;

                                    SkinManager.SliderManager.Draw(lineList, HitObjectManager.HitObjectRadius * GameBase.GamefieldRatio, DisableColourRotation ? 0 : index, null);
                                }
                                break;
                        }

                        Vector2 scale = new Vector2(HitObjectManager.SpriteRatio, HitObjectManager.SpriteRatio);

                        pTexture hitcircle = SkinManager.Load("hitcircle");
                        pTexture hitcircleoverlay = SkinManager.Load("hitcircleoverlay");


                        Gl.glColorMask(255, 255, 255, 0);
                        Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);

                        hitcircle.TextureGl.Draw(pos2, centre, Colour, scale, 0,
                                                 new Rectangle(0, 0, hitcircle.Width, hitcircle.Height),
                                                 SpriteEffects.None);
                        hitcircleoverlay.TextureGl.Draw(pos2, centre, Color.White, scale, 0,
                                                        new Rectangle(0, 0, hitcircle.Width, hitcircle.Height),
                                                        SpriteEffects.None);

                        hitcircle.TextureGl.Draw(pos1, centre, Colour, scale, 0,
                                                 new Rectangle(0, 0, hitcircle.Width, hitcircle.Height),
                                                 SpriteEffects.None);
                        hitcircleoverlay.TextureGl.Draw(pos1, centre, Color.White, scale, 0,
                                                        new Rectangle(0, 0, hitcircle.Width, hitcircle.Height),
                                                        SpriteEffects.None);

                        Gl.glColorMask(0, 0, 0, 255);
                        Gl.glBlendFunc(Gl.GL_ONE, Gl.GL_ONE_MINUS_SRC_ALPHA);

                        hitcircle.TextureGl.Draw(pos2, centre, Colour, scale, 0,
                                                 new Rectangle(0, 0, hitcircle.Width, hitcircle.Height),
                                                 SpriteEffects.None);
                        hitcircleoverlay.TextureGl.Draw(pos2, centre, Color.White, scale, 0,
                                                        new Rectangle(0, 0, hitcircle.Width, hitcircle.Height),
                                                        SpriteEffects.None);

                        hitcircle.TextureGl.Draw(pos1, centre, Colour, scale, 0,
                                                 new Rectangle(0, 0, hitcircle.Width, hitcircle.Height),
                                                 SpriteEffects.None);
                        hitcircleoverlay.TextureGl.Draw(pos1, centre, Color.White, scale, 0,
                                                        new Rectangle(0, 0, hitcircle.Width, hitcircle.Height),
                                                        SpriteEffects.None);


                        Gl.glColorMask(255, 255, 255, 255);

                        int textureId = 0;
                        int[] textures = new int[1];
                        Gl.glGenTextures(1, textures);
                        textureId = textures[0];

                        Gl.glEnable(GlControl.SurfaceType);

                        Gl.glBindTexture(GlControl.SurfaceType, textureId);
                        Gl.glTexParameteri(GlControl.SurfaceType, Gl.GL_TEXTURE_MIN_FILTER, (int)Gl.GL_LINEAR);
                        Gl.glTexParameteri(GlControl.SurfaceType, Gl.GL_TEXTURE_MAG_FILTER, (int)Gl.GL_LINEAR);

                        Gl.glCopyTexImage2D(GlControl.SurfaceType, 0, Gl.GL_RGBA, 0, 0, xSize, ySize, 0);
                        Gl.glDisable(GlControl.SurfaceType);


                        if (sliderBody == null)
                        {
                            TextureGl gl = new TextureGl(width, height);
                            gl.SetData(textureId);
                            pTexture pt = new pTexture(null, gl, width, height);

                            sliderBody =
                                new pSprite(pt, FieldTypes.Native, OriginTypes.TopLeft,
                                            ClockTypes.Audio, new Vector2(left, top),
                                            SpriteManager.drawOrderBwd(EndTime + 10),
                                            false, Color.White);

                            sliderBody.Disposable = true;
                            SpriteCollection.Add(sliderBody);
                            spriteManager.Add(sliderBody);
                        }
                        else
                        {
                            TextureGl gl = new TextureGl(xSize, ySize);
                            gl.SetData(textureId);
                            sliderBody.Texture = new pTexture(null, gl, xSize, ySize);

                            sliderBody.CurrentPosition = new Vector2(left, top);
                            sliderBody.Depth = SpriteManager.drawOrderBwd(EndTime + 10);
                        }

                        //restore viewport
                        GlControl.ResetViewport();
                    }

                    sliderBody.Transformations.Clear();

                    sliderBody.Transformations.Add(
                        new Transformation(TransformationType.Fade, 0, 0.97F, StartTime - HitObjectManager.PreEmpt,
                                           StartTime - HitObjectManager.PreEmpt + HitObjectManager.FadeIn));
                    sliderBody.Transformations.Add(
                        new Transformation(TransformationType.Fade, 0.97F, 0, EndTime,
                                           EndTime + HitObjectManager.FadeOut));

                    if (GameBase.Mode != OsuModes.Edit)
                    {
                        Dimmed = true;
                        if (fullyDrawn)
                            DimCollection.Add(sliderBody);
                        sliderBody.StartColour = HitObjectManager.Grayish;
                    }
                }
            }
            else
                RemoveBody();
        }

        private void RemoveBody()
        {
            if (sliderBody != null && sliderBody.Texture != null)
            {
                sliderBody.Texture.Dispose();
                sliderBody.Texture = null;
            }
        }

        #endregion

        #region Placement

        private bool allowNewPlacing = true;
        private bool placingPending;
        private Vector2 placingPoint;
        internal bool placingValid;

        private bool successfulPlace;
        private int successfulPlaceLength;
        internal bool unifiedSoundAddition = true;

        internal override void SetColour(Color color)
        {
            if (color != Colour)
            {
                Colour = color;
                ColourDim =
                    new Color((byte)Math.Max(0, color.R * 0.75F), (byte)Math.Max(0, color.G * 0.75F),
                              (byte)Math.Max(0, color.B * 0.75F), 255);
                sliderStartCircle.SetColour(Colour);
                foreach (HitCircleSliderEnd c in sliderEndCircles)
                    c.SetColour(Colour);
                RemoveBody();
            }
        }

        internal override void Select()
        {
            if (sliderBody != null)
                sliderBody.Dispose();

            if (!sliderStartCircle.Selected)
            {
                sliderStartCircle.SpriteSelectionCircle.StartColour = Color.White;
                sliderStartCircle.Select();
                //Use a blue highlight in editor when not selecting a specific endpoint.
            }

            if (sliderEndCircles.Count > 0 && !sliderEndCircles[0].Selected)
            {
                sliderEndCircles[0].SpriteSelectionCircle.StartColour = Color.White;
                sliderEndCircles[0].Select();
                //Do the same for the "other end"
            }
        }

        internal override void Deselect()
        {
            if (sliderBody != null)
                sliderBody.Dispose();
            sliderAllCircles.ForEach(hc =>
                                         {
                                             hc.Selected = false;
                                             hc.Deselect();
                                         });
        }

        internal override void ModifyTime(int newTime)
        {
            ModifySliderTime(newTime, true);
        }

        internal void ModifySliderTime(int newTime, bool doUpdate)
        {
            sliderStartCircle.ModifyTime(newTime);
            StartTime = newTime;

            if (doUpdate)
                UpdateCalculations(true);
            else
            {
                //Special case to force update at next hitobjectmanager update.
                EndTime = newTime;
                newCache = true;
            }
        }

        internal override void ModifyPosition(Vector2 newPosition)
        {
            Vector2 change = newPosition - Position;

            for (int i = 0; i < sliderCurvePoints.Count; i++)
                sliderCurvePoints[i] += change;
            if (sliderCurveSmoothLines != null)
                for (int i = 0; i < sliderCurveSmoothLines.Count; i++)
                {
                    sliderCurveSmoothLines[i].p1 += change;
                    sliderCurveSmoothLines[i].p2 += change;
                    sliderCurveSmoothLines[i].Recalc();
                }


            foreach (pSprite p in SpriteCollection)
            {
                if (p.Field == FieldTypes.Gamefield512x384)
                {
                    p.StartPosition += change;
                    p.CurrentPosition += change;
                }
                else
                {
                    p.StartPosition += change * GameBase.WindowRatio * GameBase.GamefieldRatioWindowIndependent;
                    p.CurrentPosition += change * GameBase.WindowRatio * GameBase.GamefieldRatioWindowIndependent;
                }
            }

            if (sliderBall != null)
                foreach (Transformation t in sliderBall.Transformations)
                    if (t.Type == TransformationType.Movement)
                    {
                        t.StartVector += change;
                        t.EndVector += change;
                    }

            Position = newPosition;
            Position2 += change;

            //sliderStartCircle.Position = newPosition;
            sliderStartCircle.ModifyPosition(newPosition);
            sliderEndCircles.ForEach(s => s.ModifyPosition(s.Position + change));

            EndPosition += change;
        }

        internal void AdjustRepeats2(int time)
        {
            int slideTime = (EndTime - StartTime) / sliderRepeatCount;

            if (slideTime == 0 || time <= StartTime || sliderCurvePoints.Count < 2)
                return;

            int i = 1;
            int addedTime = StartTime + slideTime;
            while (addedTime < time)
            {
                addedTime += slideTime;
                i++;
            }

            if (sliderRepeatCount != i)
            {
                sliderRepeatCount = i;
                UpdateCalculations(true);
            }

            SoundTypeList.Clear();
            unifiedSoundAddition = true;
        }

        internal void AdjustRepeats(int time)
        {
            int slideTime = (EndTime - StartTime) / sliderRepeatCount;

            if (slideTime == 0 || time <= StartTime)
                return;

            int i = 1;
            int addedTime = StartTime + (int)(slideTime * 1.5);
            while (addedTime < time)
            {
                addedTime += slideTime;
                i++;
            }

            if (sliderRepeatCount != i)
            {
                sliderRepeatCount = i;
                UpdateCalculations(true);
            }

            SoundTypeList.Clear();
            unifiedSoundAddition = true;
        }

        internal void PlacePoint(bool last, bool remove)
        {
            if (sliderCurvePoints.Count > 1)
            {
                if (!placingValid && !last && remove)
                    sliderCurvePoints.RemoveAt(sliderCurvePoints.Count - 1);
                else if (placingValid && sliderCurveSmoothLines != null)
                {
                    //if (!last)
                    //    sliderCurvePoints[sliderCurvePoints.Count - 1] =
                    //        sliderCurveSmoothLines[sliderCurveSmoothLines.Count - 1].p2;
                    placingValid = false;
                    successfulPlace = true;
                }
                else if (!remove && !placingValid &&
                         sliderCurvePoints[sliderCurvePoints.Count - 1] !=
                         sliderCurvePoints[sliderCurvePoints.Count - 2])
                {
                    sliderCurvePoints.Add(sliderCurvePoints[sliderCurvePoints.Count - 1]);
                    placingValid = false;
                    successfulPlace = true;
                }
            }

            allowNewPlacing = !last;
            UpdateCalculations(true);
        }

        internal void PlacePointNext(Vector2 sliderPlacementPoint)
        {
            if (!allowNewPlacing)
                return;

            if (placingValid && sliderCurvePoints[sliderCurvePoints.Count - 1] == placingPoint)
                sliderCurvePoints.RemoveAt(sliderCurvePoints.Count - 1);

            placingPoint = sliderPlacementPoint;
            placingValid = false;
            placingPending = true;
        }

        #endregion

        #region Positioning

        internal int timeAtLength(float length)
        {
            return (int)(StartTime + (length / Velocity) * 1000);
        }

        internal Vector2 currentPosition()
        {
            return positionAtTime(AudioEngine.Time);
        }

        internal Vector2 positionAtTime(int time)
        {
            float pos = (time - StartTime) / ((float)(EndTime - StartTime) / sliderRepeatCount);

            if (pos % 2 > 1)
                pos = 1 - (pos % 1);
            else
                pos = (pos % 1);

            float lengthRequired = (float)(sliderLength * pos);
            return positionAtLength(lengthRequired);
        }

        internal Vector2 positionAtLength(float length)
        {
            float lengthCurrent = 0;
            int i = 0;
            foreach (Line l in sliderCurveSmoothLines)
            {
                if (lengthCurrent >= length)
                    break;
                lengthCurrent += Vector2.Distance(l.p1, l.p2);
                i++;
            }

            if (sliderCurveSmoothLines.Count == 0)
                return Position;

            if (i == 0)
                return sliderCurveSmoothLines[i].p1;

            Vector2 linearAdd = (sliderCurveSmoothLines[i - 1].p2 - sliderCurveSmoothLines[i - 1].p1)
                                *
                                (1 -
                                 (Math.Abs(length - lengthCurrent)) /
                                 Vector2.Distance(sliderCurveSmoothLines[i - 1].p1,
                                                  sliderCurveSmoothLines[i - 1].p2));

            return sliderCurveSmoothLines[i - 1].p1 + linearAdd;
        }

        #endregion

        #region Scoring

        private MouseButtons downButton = MouseButtons.None; //The mouse button pressed to begin sliding.
        protected HitObjectManager hitObjectManager;
        internal int sliderTicksHit;
        private int InitTime;
        internal bool IsSliding;
        private int lastTickSounded;
        internal int sliderTicksMissed;
        internal List<int> sliderRepeatPoints = new List<int>();

        internal List<int> sliderScoreTimingPoints = new List<int>();
        public List<HitObjectSoundType> SoundTypeList = new List<HitObjectSoundType>();
        internal double Velocity;
        private SpriteEffects startSpriteEffect;

        internal override Vector2 Position2 { get; set; }

        internal virtual bool StartIsHit
        {
            get { return sliderStartCircle.IsHit; }
        }

        internal virtual IncreaseScoreType HitStart()
        {
            if (InputManager.leftCond || InputManager.rightCond)
                //If no button is registered as the slider-down butt, register one now
                downButton = InputManager.leftCond
                                 ? MouseButtons.Left
                                 : InputManager.rightCond ? MouseButtons.Right : MouseHandler.MouseDownButton;
            return sliderStartCircle.Hit();
        }

        internal override IncreaseScoreType Hit()
        {
            IsHit = true;
            StopSound();

            if (sliderStartCircle.hitValue > 0)
                sliderTicksHit++;

            if (IsSliding)
            {
                HitCircleSliderEnd h =
                    new HitCircleSliderEnd(
                        (EndPosition == Position ? Position2 : Position), EndTime, EndTime, false, 0, this);
                h.SetColour(Colour);
                h.Arm();
                spriteManager.Add(h.SpriteCollection);
            }

            double hitFraction = (double)sliderTicksHit / (sliderScoreTimingPoints.Count + 1);

            if (hitFraction > 0)
                PlaySound();

            if (hitFraction == 1)
                return IncreaseScoreType.Hit300;
            if (hitFraction >= 0.5)
                return IncreaseScoreType.Hit100;
            if (hitFraction > 0)
                return IncreaseScoreType.Hit50;
            return IncreaseScoreType.Miss;
        }

        internal override void PlaySound()
        {
            // endIndex == 0 tells us to use the i46 fix so it must have the correct value even when unifiedSoundAddition is true.
            PlaySound(false, AudioEngine.Time > StartTime + (EndTime - StartTime) / 2 ? SoundTypeList.Count - 1 : 0);
            //This crap is here because edit mode autoplay is pretty crap itself.
        }

        protected override float PositionalSound { get { return sliderBall.CurrentPosition.X / 512f - 0.5f; } }

        internal void PlaySound(bool tickOnly, int endIndex)
        {
            if (AudioEngine.Time <= EndTime)
                SliderSoundStart();

            if (tickOnly)
            {
                ControlPoint pt = AudioEngine.controlPointAt(sliderScoreTimingPoints[endIndex] + 2);

                AudioEngine.PlayTickSamples(new HitSoundInfo(SoundType, pt.sampleSet, pt.customSamples, pt.volume), PositionalSound, true);
            }
            else
            {
                if (endIndex == 0)
                {
                    HitObjectManager.OnHitSound(SoundType);

                    ControlPoint pt = AudioEngine.controlPointAt(StartTime);

                    // The slider start is i46-proof.
                    AudioEngine.PlayHitSamples(new HitSoundInfo(unifiedSoundAddition ? SoundType : SoundTypeList[0], pt.sampleSet, pt.customSamples, pt.volume), PositionalSound, true);
                }
                else if (unifiedSoundAddition)
                {
                    HitObjectManager.OnHitSound(SoundType);

                    ControlPoint pt;

                    if ((endIndex >= this.sliderRepeatCount) || (endIndex == -1))
                        // Add 2ms fudge to avoid rounding error: Rebounds placed exactly on section rollovers should use the NEW section's sampleset.
                        pt = AudioEngine.controlPointAt(EndTime + 2);
                    else
                        pt = AudioEngine.controlPointAt(sliderRepeatPoints[endIndex - 1] + 2);

                    // Slider rebounds & ends are i46-proof too now.
                    AudioEngine.PlayHitSamples(new HitSoundInfo(SoundType, pt.sampleSet, pt.customSamples, pt.volume), PositionalSound, true);
                }
                else
                {
                    if (endIndex >= SoundTypeList.Count)
                        return;

                    HitObjectManager.OnHitSound(SoundTypeList[endIndex]);

                    ControlPoint pt;

                    if ((endIndex >= this.sliderRepeatCount) || (endIndex == -1))
                        pt = AudioEngine.controlPointAt(EndTime + 2);
                    else
                        pt = AudioEngine.controlPointAt(sliderRepeatPoints[endIndex - 1] + 2);

                    AudioEngine.PlayHitSamples(new HitSoundInfo(SoundTypeList[endIndex], pt.sampleSet, pt.customSamples, pt.volume), PositionalSound, true);
                }
            }
        }

        internal void SliderSoundStart()
        {
            if ((SkinManager.Current.LayeredHitSounds || (SoundType & HitObjectSoundType.Whistle) == 0) &&
                BASSActive.BASS_ACTIVE_PLAYING != Bass.BASS_ChannelIsActive(AudioEngine.ch_sliderSlide))
            {
                Bass.BASS_ChannelSetAttribute(AudioEngine.ch_sliderSlide, BASSAttribute.BASS_ATTRIB_VOL,
                                              (float)AudioEngine.VolumeSample / 100);
                Bass.BASS_ChannelPlay(AudioEngine.ch_sliderSlide, true);
            }

            if ((SoundType & HitObjectSoundType.Whistle) > 0 &&
                BASSActive.BASS_ACTIVE_PLAYING != Bass.BASS_ChannelIsActive(AudioEngine.ch_sliderWhistle))
            {
                Bass.BASS_ChannelSetAttribute(AudioEngine.ch_sliderWhistle, BASSAttribute.BASS_ATTRIB_VOL,
                                              (float)AudioEngine.VolumeSample / 100);
                Bass.BASS_ChannelPlay(AudioEngine.ch_sliderWhistle, true);
            }



            Bass.BASS_ChannelSetAttribute(AudioEngine.ch_sliderSlide, BASSAttribute.BASS_ATTRIB_PAN, PositionalSound * 0.4f);
            Bass.BASS_ChannelSetAttribute(AudioEngine.ch_sliderWhistle, BASSAttribute.BASS_ATTRIB_PAN, PositionalSound * 0.4f);

        }

        internal override void StopSound()
        {
            Bass.BASS_ChannelPause(AudioEngine.ch_sliderSlide);
            Bass.BASS_ChannelPause(AudioEngine.ch_sliderWhistle);
        }

        internal void InitSlide()
        {
            InitTime = AudioEngine.Time;

            sliderFollower.Transformations.RemoveAll(
                delegate(Transformation t) { return t.Type != TransformationType.Movement; });

            int time;
            if (GameBase.Mode == OsuModes.Edit)
                time = StartTime;
            else
                time = Math.Max(AudioEngine.Time, StartTime);

            sliderFollower.Transformations.Add(
                new Transformation(TransformationType.Fade, 0, 1, time, Math.Min(EndTime, time + 60)));
            sliderFollower.Transformations.Add(
                new Transformation(TransformationType.Scale, 0.5F, 1, time, Math.Min(EndTime, time + 180),
                                   EasingTypes.In));

            sliderFollower.Transformations.Add(new Transformation(TransformationType.Fade, 1, 0, EndTime, EndTime + 200,
                                                                  EasingTypes.Out));
            sliderFollower.Transformations.Add(
                new Transformation(TransformationType.Scale, 1, 0.8f, EndTime, EndTime + 200, EasingTypes.In));

            int count = sliderScoreTimingPoints.Count;
            int fadeout = 200;

            int delay = count > 1 ? Math.Min(fadeout, sliderScoreTimingPoints[1] - sliderScoreTimingPoints[0]) : fadeout;
            float ratio = 1.1f - ((float)delay / fadeout) * 0.1f;
            if (count > 0)
                foreach (int i in sliderScoreTimingPoints.GetRange(0, count - 1))
                    sliderFollower.Transformations.Add(
                        new Transformation(TransformationType.Scale, 1.1F, ratio, i,
                                           Math.Min(EndTime, i + delay)));

            sliderEndCircles.ForEach(delegate(HitCircleSliderEnd h) { h.Arm(); });

            IsSliding = true;
        }

        internal void KillSlide()
        {
            if (!IsSliding)
                return;

            sliderFollower.Transformations.RemoveAll(
                delegate(Transformation t) { return t.Type != TransformationType.Movement; });

            int nextScorePoint = EndTime;
            foreach (int i in sliderScoreTimingPoints)
                if (i > AudioEngine.Time)
                {
                    nextScorePoint = i;
                    break;
                }

            sliderFollower.Transformations.Add(
                new Transformation(TransformationType.Fade, 1, 0, nextScorePoint - 100, nextScorePoint));
            sliderFollower.Transformations.Add(
                new Transformation(TransformationType.Scale, 1, 2, nextScorePoint - 100, nextScorePoint));

            if (!IsHit)
                sliderEndCircles.ForEach(delegate(HitCircleSliderEnd h) { h.Disarm(); });


            IsSliding = false;
        }

        internal override IncreaseScoreType GetScorePoints(Vector2 currentMousePos)
        {
            if (IsHit || AudioEngine.Time < StartTime)
                return IncreaseScoreType.Ignore;

            bool allowable = false;

            bool mouseDownAcceptable = false;
            bool mouseDownAcceptableSwap = MouseHandler.GameDownState &&
                                           !(MouseHandler.LastButton == (MouseButtons.Left | MouseButtons.Right) &&
                                             MouseHandler.LastButton2 == MouseHandler.MouseDownButton);

            if (MouseHandler.GameDownState)
            {
                if (downButton == MouseButtons.None ||
                    (MouseHandler.MouseDownButton != (MouseButtons.Left | MouseButtons.Right) && mouseDownAcceptableSwap))
                {
                    downButton = InputManager.leftCond
                                     ? MouseButtons.Left
                                     : InputManager.rightCond ? MouseButtons.Right : MouseHandler.MouseDownButton;
                    mouseDownAcceptable = true;
                }
                else if ((MouseHandler.MouseDownButton & downButton) > 0)
                    //Otherwise check if the correct button is down.
                    mouseDownAcceptable = true;
            }
            else
                downButton = MouseButtons.None;

            mouseDownAcceptable |= mouseDownAcceptableSwap | Player.Relaxing;

            if (mouseDownAcceptable)
            {
                //TODO: mm, this can't be good
                float radius = (IsSliding ? HitObjectManager.HitObjectRadius * 2.4F : HitObjectManager.HitObjectRadius) *
                               GameBase.GamefieldRatio;

#if TOUCHSCREEN
                int offsetTime = AudioEngine.Time - (InputManager.TouchDevice && !ModManager.ModAutoplay ? ConfigManager.sTouchDragOffset : 0);
#else
                int offsetTime = AudioEngine.Time;
#endif
                //DON'T FUCKIGN TOUCH THIS
                Transformation f = sliderBall.Transformations.Find(t => t.Time1 <= offsetTime &&
                                                                        t.Time2 >= offsetTime);
                if (f == null && offsetTime > sliderBall.Transformations[sliderBall.Transformations.Count - 1].Time2)
                    f = sliderBall.Transformations[sliderBall.Transformations.Count - 1];

#if TOUCHSCREEN
                //Special case for when drag offset causes timing to end up before the slider begins.
                if (f == null && offsetTime < StartTime && AudioEngine.Time >= StartTime)
                    f = sliderBall.Transformations[0];
#endif

                if (f != null)
                {
                    Vector2 pos;
                    if (f.Time2 == f.Time1)
                        pos = f.EndVector;
                    else
                        pos = f.StartVector +
                              (f.EndVector - f.StartVector) *
                              (1 - (float)(f.Time2 - offsetTime) / (f.Time2 - f.Time1));
                    allowable =
                        Vector2.DistanceSquared(currentMousePos, GameBase.GamefieldToDisplay(pos)) < radius * radius;

                }
            }

            if (allowable && !IsSliding)
            {
                InitSlide();
            }

            IncreaseScoreType score = IncreaseScoreType.Ignore;
            int pointCount = 0;

            while (pointCount < sliderScoreTimingPoints.Count &&
                   sliderScoreTimingPoints[pointCount] <= AudioEngine.Time)
                pointCount++;

            if (sliderTicksHit + sliderTicksMissed < pointCount)
            {
                if (allowable && InitTime <= sliderScoreTimingPoints[sliderTicksHit + sliderTicksMissed])
                {
                    sliderTicksHit++;
                    if (sliderScoreTimingPoints.Count == pointCount)
                        score = IncreaseScoreType.SliderEnd;
                    else if (pointCount % (sliderScoreTimingPoints.Count / sliderRepeatCount) == 0)
                        score = IncreaseScoreType.SliderRepeat;
                    else
                        score = IncreaseScoreType.SliderTick;
                }
                else
                {
                    sliderTicksMissed++;
                    if (sliderTicksHit + sliderTicksMissed == sliderScoreTimingPoints.Count)
                        score = IncreaseScoreType.MissHpOnlyNoCombo;
                    else
                        score = IncreaseScoreType.MissHpOnly;
                }
            }

            if (!allowable && IsSliding && sliderTicksHit + sliderTicksMissed < sliderScoreTimingPoints.Count)
                KillSlide();

            return score;
        }

        #endregion
    }

    internal enum CurveTypes
    {
        Catmull,
        Bezier,
        Linear
    } ;
}