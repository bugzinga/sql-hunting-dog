using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HuntingDog
{
    public partial class CircularProgressControl : UserControl
    {
        #region Constants

        private const int DEFAULT_INTERVAL = 60;
        private readonly Color DEFAULT_TICK_COLOR = Color.FromArgb(58, 58, 58);
        private const int DEFAULT_TICK_WIDTH = 2;
        private const int MINIMUM_INNER_RADIUS = 4;
        private const int MINIMUM_OUTER_RADIUS = 8;
        private Size MINIMUM_CONTROL_SIZE = new Size(28, 28);
        private const int MINIMUM_PEN_WIDTH = 2;

        #endregion

        #region Enums

        public enum Direction
        {
            CLOCKWISE,
            ANTICLOCKWISE
        }

        #endregion

        #region Members

        private int m_Interval;
        Pen m_Pen = null;
        PointF m_CentrePt = new PointF();
        int m_InnerRadius = 0;
        int m_OuterRadius = 0;
        int m_StartAngle = 0;
        int m_AlphaStartValue = 0;
        int m_SpokesCount = 0;
        int m_AngleIncrement = 0;
        int m_AlphaDecrement = 0;
        Timer m_Timer = null;

        #endregion

        #region Properties

        /// <summary>
        /// Time interval for each tick.
        /// </summary>
        public int Interval
        {
            get
            {
                return m_Interval;
            }
            set
            {
                if (value > 0)
                {
                    m_Interval = value;
                }
                else
                {
                    m_Interval = DEFAULT_INTERVAL;
                }
            }
        }

        /// <summary>
        /// Color of the tick
        /// </summary>
        public Color TickColor { get; set; }

        /// <summary>
        /// Direction of rotation - CLOCKWISE/ANTICLOCKWISE
        /// </summary>
        public Direction Rotation { get; set; }

        /// <summary>
        /// Angle at which the tick should start
        /// </summary>
        public int StartAngle
        {
            get
            {
                return m_StartAngle;
            }
            set
            {
                m_StartAngle = value;
            }
        }

        #endregion

        #region Construction/Initialization

        /// <summary>
        /// Ctor
        /// </summary>
        public CircularProgressControl()
        {
            this.DoubleBuffered = true;

            InitializeComponent();

            this.BackColor = Color.Transparent;
            this.TickColor = DEFAULT_TICK_COLOR;
            this.MinimumSize = MINIMUM_CONTROL_SIZE;
            this.Interval = DEFAULT_INTERVAL;
            // Default rotation direction is clockwise
            this.Rotation = Direction.CLOCKWISE;
            // Default starting angle is 12 o'clock
            this.StartAngle = 270;
            // Default number of Spokes in this control is 12
            m_SpokesCount = 12;
            // Default alpha value of the first spoke is 255
            m_AlphaStartValue = 255;
            // Calculate the angle between adjacent spokes
            m_AngleIncrement = (int)(360/m_SpokesCount);
            // Calculate the change in alpha between adjacent spokes
            m_AlphaDecrement = (int)((m_AlphaStartValue - 15) / m_SpokesCount);

            m_Pen = new Pen(TickColor, DEFAULT_TICK_WIDTH);
            m_Pen.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            m_Pen.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            m_Timer = new Timer();
            m_Timer.Interval = this.Interval;
            m_Timer.Tick += new EventHandler(OnTimerTick);
        }

        #endregion

        #region EventHandlers

        /// <summary>
        /// Handle the Tick event
        /// </summary>
        /// <param name="sender">Timer</param>
        /// <param name="e">EventArgs</param>
        void OnTimerTick(object sender, EventArgs e)
        {
            if (Rotation == Direction.CLOCKWISE)
            {
                m_StartAngle += m_AngleIncrement;

                if (m_StartAngle >= 360)
                    m_StartAngle = 0;
            }
            else if (Rotation == Direction.ANTICLOCKWISE)
            {
                m_StartAngle -= m_AngleIncrement;

                if (m_StartAngle <= -360)
                    m_StartAngle = 0;
            }

            Invalidate();
        }

        /// <summary>
        /// Handles the Paint Event of the control
        /// </summary>
        /// <param name="e">PaintEventArgs</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            // All the paintin will be handled by us.
            //base.OnPaint(e);

            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            // Since the Rendering of the spokes is dependent upon the current size of the 
            // control, the following calculation needs to be done within the Paint eventhandler.
            int alpha = m_AlphaStartValue;
            int angle = m_StartAngle;
            // Calculate the location around which the spokes will be drawn
            int width = (this.Width < this.Height) ? this.Width : this.Height;
            m_CentrePt = new PointF(this.Width / 2, this.Height / 2);
            // Calculate the width of the pen which will be used to draw the spokes
            m_Pen.Width = (int)(width / 15);
            if (m_Pen.Width < MINIMUM_PEN_WIDTH)
                m_Pen.Width = MINIMUM_PEN_WIDTH;
            // Calculate the inner and outer radii of the control. The radii should not be less than the
            // Minimum values
            m_InnerRadius = (int)(width * (140 / (float)800));
            if (m_InnerRadius < MINIMUM_INNER_RADIUS)
                m_InnerRadius = MINIMUM_INNER_RADIUS;
            m_OuterRadius = (int)(width * (250 / (float)800));
            if (m_OuterRadius < MINIMUM_OUTER_RADIUS)
                m_OuterRadius = MINIMUM_OUTER_RADIUS;

            // Render the spokes
            for (int i = 0; i < m_SpokesCount; i++)
            {
                PointF pt1 = new PointF(m_InnerRadius * (float)Math.Cos(ConvertDegreesToRadians(angle)), m_InnerRadius * (float)Math.Sin(ConvertDegreesToRadians(angle)));
                PointF pt2 = new PointF(m_OuterRadius * (float)Math.Cos(ConvertDegreesToRadians(angle)), m_OuterRadius * (float)Math.Sin(ConvertDegreesToRadians(angle)));

                pt1.X += m_CentrePt.X;
                pt1.Y += m_CentrePt.Y;
                pt2.X += m_CentrePt.X;
                pt2.Y += m_CentrePt.Y;
                m_Pen.Color = Color.FromArgb(alpha, this.TickColor);
                e.Graphics.DrawLine(m_Pen, pt1, pt2);

                if (Rotation == Direction.CLOCKWISE)
                {
                    angle -= m_AngleIncrement;
                }
                else if (Rotation == Direction.ANTICLOCKWISE)
                {
                    angle += m_AngleIncrement;
                }

                //if (i < 5)
                //    alpha -= 45;
                alpha -= m_AlphaDecrement;
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Converts Degrees to Radians
        /// </summary>
        /// <param name="degrees">Degrees</param>
        /// <returns></returns>
        private double ConvertDegreesToRadians(int degrees)
        {
            return ((Math.PI / (double)180) * degrees);
        }

        #endregion

        #region APIs

        /// <summary>
        /// Start the Tick Control rotation
        /// </summary>
        public void Start()
        {
            if (m_Timer != null)
            {
                m_Timer.Interval = this.Interval;
                m_Timer.Enabled = true;
            }
        }

        /// <summary>
        /// Stop the Tick Control rotation
        /// </summary>
        public void Stop()
        {
            if (m_Timer != null)
            {
                m_Timer.Enabled = false;
            }
        }

        #endregion
    }
}
