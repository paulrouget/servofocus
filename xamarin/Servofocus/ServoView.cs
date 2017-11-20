﻿using System;
using ServoSharp;
using Xamarin.Forms;

namespace Servofocus
{
    public class ServoView : View
    {
        public readonly Servo Servo;

        public ServoView()
        {
            Servo = new Servo();
        }
    }
}