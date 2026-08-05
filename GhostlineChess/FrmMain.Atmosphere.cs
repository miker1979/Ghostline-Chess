using System.Drawing;
using System.Drawing.Drawing2D;

namespace GhostlineChess
{
    /// <summary>
    /// Paints a restrained ruined-cathedral background
    /// behind the playable interface.
    /// </summary>
    public partial class FrmMain
    {
        /// <summary>
        /// Draws the atmospheric gradient, moon glow,
        /// and distant Gothic arches.
        /// </summary>
        protected override void OnPaintBackground(
            PaintEventArgs e)
        {
            Rectangle bounds = ClientRectangle;

            using LinearGradientBrush backgroundBrush =
                new LinearGradientBrush(
                    bounds,
                    Color.FromArgb(23, 15, 26),
                    Color.FromArgb(5, 6, 10),
                    LinearGradientMode.Vertical);

            e.Graphics.FillRectangle(
                backgroundBrush,
                bounds);

            e.Graphics.SmoothingMode =
                SmoothingMode.AntiAlias;

            using SolidBrush moonGlow =
                new SolidBrush(
                    Color.FromArgb(
                        24,
                        196,
                        209,
                        214));

            e.Graphics.FillEllipse(
                moonGlow,
                ClientSize.Width - 300,
                40,
                240,
                240);

            using Pen archPen =
                new Pen(
                    Color.FromArgb(
                        46,
                        177,
                        136,
                        58),
                    5F);

            for (int index = 0;
                 index < 4;
                 index++)
            {
                int x = 25 + index * 300;

                Rectangle arch =
                    new Rectangle(
                        x,
                        155,
                        220,
                        560);

                e.Graphics.DrawArc(
                    archPen,
                    arch,
                    180F,
                    180F);

                e.Graphics.DrawLine(
                    archPen,
                    arch.Left,
                    arch.Top + arch.Height / 2,
                    arch.Left,
                    arch.Bottom);

                e.Graphics.DrawLine(
                    archPen,
                    arch.Right,
                    arch.Top + arch.Height / 2,
                    arch.Right,
                    arch.Bottom);
            }
        }
    }
}
