﻿using System;

namespace AutoClicker
{
    public partial class Form1
    {
        private class WindowItem
        {
            public IntPtr Handle { get; set; }
            public string Title { get; set; }

            public override string ToString()
            {
                return Title;
            }
        }
    }
}
