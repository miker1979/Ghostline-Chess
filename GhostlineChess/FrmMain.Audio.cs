using System.Drawing;
using System.Windows.Forms;
using GhostlineChess.Audio;
using GhostlineChess.Enums;
using GhostlineChess.GameLogic;

namespace GhostlineChess
{
    /// <summary>
    /// Connects native audio playback and controls
    /// to completed Ghostline Chess game events.
    /// </summary>
    public partial class FrmMain
    {
        private readonly AudioManager audioManager =
            new AudioManager(
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Assets",
                    "Audio"));

        private readonly Label audioLabel =
            new Label();

        private readonly TrackBar volumeTrackBar =
            new TrackBar();

        private readonly Button muteButton =
            new Button();

        private readonly System.Windows.Forms.Timer
            environmentalSoundTimer =
                new System.Windows.Forms.Timer();

        private readonly Random audioRandom =
            new Random();

        private readonly
            (string FileName, float RelativeVolume)[]
                environmentalSounds =
                {
                    ("Creak_Long_Distant.wav", 0.22F),
                    ("Creak_House_Distant.wav", 0.30F),
                    ("Creak_Door_Distant.wav", 0.34F),
                    ("Creak_Drawer_Distant.wav", 0.30F)
                };

        private int lastEnvironmentalSoundIndex = -1;
        private bool hasPlayedEnvironmentalSound;
        private AudioTensionLevel currentAudioTension =
            AudioTensionLevel.Dormant;

        private enum AudioTensionLevel
        {
            Dormant,
            Ominous,
            Doom
        }

        /// <summary>
        /// Adds volume and mute controls below the
        /// primary buttons and starts the ambience.
        /// </summary>
        private void InitializeAudioExperience()
        {
            int controlY =
                newGameButton.Bottom + 8;

            audioLabel.Text =
                "AUDIO";

            audioLabel.Location =
                new Point(
                    boardFramePanel.Left,
                    controlY + 4);

            audioLabel.Size =
                new Size(65, 28);

            audioLabel.Font =
                new Font(
                    "Georgia",
                    9F,
                    FontStyle.Bold);

            audioLabel.ForeColor =
                gothicGold;

            audioLabel.BackColor =
                Color.Transparent;

            volumeTrackBar.Minimum = 0;
            volumeTrackBar.Maximum = 100;
            volumeTrackBar.Value = 55;
            volumeTrackBar.TickFrequency = 10;

            volumeTrackBar.Location =
                new Point(
                    audioLabel.Right,
                    controlY);

            volumeTrackBar.Size =
                new Size(170, 40);

            volumeTrackBar.BackColor =
                gothicBackground;

            volumeTrackBar.Scroll +=
                VolumeTrackBar_Scroll;

            muteButton.Text =
                "Mute";

            muteButton.Location =
                new Point(
                    volumeTrackBar.Right + 8,
                    controlY);

            muteButton.Size =
                new Size(90, 34);

            StyleGothicButton(muteButton);

            muteButton.Click +=
                MuteButton_Click;

            Controls.Add(audioLabel);
            Controls.Add(volumeTrackBar);
            Controls.Add(muteButton);

            audioLabel.BringToFront();
            volumeTrackBar.BringToFront();
            muteButton.BringToFront();

            audioManager.Volume =
                volumeTrackBar.Value;

            UpdateAudioTensionState();

            ScheduleNextEnvironmentalSound();

            environmentalSoundTimer.Tick +=
                EnvironmentalSoundTimer_Tick;

            newGameButton.Click +=
                AudioBoardStateChanged;

            loadFenButton.Click +=
                AudioBoardStateChanged;

            startingPositionButton.Click +=
                AudioBoardStateChanged;

            Shown += FrmMain_Shown;
        }

        /// <summary>
        /// Opens the audio device on a worker thread only
        /// after the main chess window has been displayed.
        /// </summary>
        private async void FrmMain_Shown(
            object? sender,
            EventArgs e)
        {
            Shown -= FrmMain_Shown;

            try
            {
                await Task.Run(
                    audioManager.Initialize);

                audioManager.StartAmbience();
                environmentalSoundTimer.Start();
            }
            catch (Exception exception)
            {
                environmentalSoundTimer.Stop();

                MessageBox.Show(
                    this,
                    "Ghostline Chess opened normally, but audio " +
                    "could not start.\n\n" +
                    exception.Message,
                    "Audio Unavailable",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Plays the correct effect after a legal
        /// move has completely updated the board.
        /// </summary>
        private async void PlayCompletedMoveAudio(
            JournalMoveContext completedMove,
            PieceType? promotionType)
        {
            UpdateAudioTensionState();

            if (chessGame.Result ==
                    GameResult.WhiteWon ||
                chessGame.Result ==
                    GameResult.BlackWon)
            {
                audioManager.PlayEffect(
                    "Slow Stinger.wav");

                return;
            }

            if (promotionType.HasValue)
            {
                audioManager.PlayEffect(
                    "Piano_crescendo.wav",
                    0.82F);

                await Task.Delay(320);

                audioManager.PlayEffect(
                    GetPieceSoundFileName(
                        completedMove.MovingColor,
                        promotionType.Value,
                        false),
                    0.72F);
            }
            else if (completedMove.IsCastling)
            {
                audioManager.PlayEffect(
                    "Swoosh_3.wav");
            }
            else
            {
                bool isCapture =
                    completedMove.CapturedType.HasValue;

                audioManager.PlayEffect(
                    GetPieceSoundFileName(
                        completedMove.MovingColor,
                        completedMove.MovingType,
                        isCapture),
                    isCapture
                        ? 0.90F
                        : 0.72F);
            }

            if (MoveValidator.IsKingInCheck(
                    chessGame.Board,
                    chessGame.Turn))
            {
                await Task.Delay(120);

                audioManager.PlayEffect(
                    "Stinger.wav",
                    0.86F);
            }
        }

        /// <summary>
        /// Builds the licensed sound filename for one
        /// faction, piece type, and completed action.
        /// </summary>
        private static string GetPieceSoundFileName(
            PieceColor movingColor,
            PieceType movingType,
            bool isCapture)
        {
            string factionName =
                movingColor == PieceColor.White
                    ? "Hallowed"
                    : "Damned";

            string actionName =
                isCapture
                    ? "Capture"
                    : "Move";

            return Path.Combine(
                "Pieces",
                $"{factionName}_{movingType}_" +
                $"{actionName}.wav");
        }

        /// <summary>
        /// Updates native playback when the volume
        /// slider is moved.
        /// </summary>
        private void VolumeTrackBar_Scroll(
            object? sender,
            EventArgs e)
        {
            audioManager.Volume =
                volumeTrackBar.Value;
        }

        /// <summary>
        /// Toggles all ambience and effects.
        /// </summary>
        private void MuteButton_Click(
            object? sender,
            EventArgs e)
        {
            audioManager.Muted =
                !audioManager.Muted;

            muteButton.Text =
                audioManager.Muted
                    ? "Unmute"
                    : "Mute";
        }

        /// <summary>
        /// Plays a restrained haunted-room detail
        /// at an irregular interval.
        /// </summary>
        private void EnvironmentalSoundTimer_Tick(
            object? sender,
            EventArgs e)
        {
            int soundIndex;

            do
            {
                soundIndex =
                    audioRandom.Next(
                        environmentalSounds.Length);
            }
            while (
                environmentalSounds.Length > 1 &&
                soundIndex ==
                    lastEnvironmentalSoundIndex);

            lastEnvironmentalSoundIndex =
                soundIndex;

            (string FileName, float RelativeVolume)
                selectedSound =
                    environmentalSounds[soundIndex];

            audioManager.PlayEffect(
                selectedSound.FileName,
                selectedSound.RelativeVolume);

            hasPlayedEnvironmentalSound = true;

            ScheduleNextEnvironmentalSound();
        }

        /// <summary>
        /// Re-evaluates audio tension after a board-reset
        /// control has finished changing the current game.
        /// </summary>
        private void AudioBoardStateChanged(
            object? sender,
            EventArgs e)
        {
            UpdateAudioTensionState(
                forceReschedule: true);
        }

        /// <summary>
        /// Raises atmosphere intensity as material leaves
        /// the board, using captured pieces rather than a
        /// fixed move number to measure the battle's danger.
        /// </summary>
        private void UpdateAudioTensionState(
            bool forceReschedule = false)
        {
            int remainingPieces = 0;

            for (int row = 0; row < 8; row++)
            {
                for (int column = 0; column < 8; column++)
                {
                    if (!chessGame.Board.Spots[
                            row,
                            column].Piece.IsEmpty)
                    {
                        remainingPieces++;
                    }
                }
            }

            int capturedPieces =
                Math.Max(
                    0,
                    32 - remainingPieces);

            AudioTensionLevel nextTension =
                capturedPieces >= 14
                    ? AudioTensionLevel.Doom
                    : capturedPieces >= 6
                        ? AudioTensionLevel.Ominous
                        : AudioTensionLevel.Dormant;

            bool tensionChanged =
                nextTension != currentAudioTension;

            currentAudioTension =
                nextTension;

            audioManager.AmbienceIntensity =
                currentAudioTension switch
                {
                    AudioTensionLevel.Ominous => 1.08F,
                    AudioTensionLevel.Doom => 1.18F,
                    _ => 1F
                };

            if ((tensionChanged || forceReschedule) &&
                environmentalSoundTimer.Enabled)
            {
                ScheduleNextEnvironmentalSound();
            }
        }

        /// <summary>
        /// Schedules an earlier first creak, followed by
        /// wider gaps that do not form a predictable rhythm.
        /// </summary>
        private void ScheduleNextEnvironmentalSound()
        {
            (int Minimum, int Maximum) delayRange =
                currentAudioTension switch
                {
                    AudioTensionLevel.Ominous =>
                        hasPlayedEnvironmentalSound
                            ? (35_000, 90_001)
                            : (20_000, 50_001),

                    AudioTensionLevel.Doom =>
                        hasPlayedEnvironmentalSound
                            ? (18_000, 45_001)
                            : (12_000, 30_001),

                    _ =>
                        hasPlayedEnvironmentalSound
                            ? (55_000, 145_001)
                            : (30_000, 75_001)
                };

            environmentalSoundTimer.Interval =
                audioRandom.Next(
                    delayRange.Minimum,
                    delayRange.Maximum);
        }

        /// <summary>
        /// Releases native audio resources when
        /// the main window closes.
        /// </summary>
        protected override void OnFormClosed(
            FormClosedEventArgs e)
        {
            environmentalSoundTimer.Stop();
            environmentalSoundTimer.Dispose();
            audioManager.Dispose();

            base.OnFormClosed(e);
        }
    }
}
