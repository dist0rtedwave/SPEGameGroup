﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Testgame
{
    class Commands
    {
        public static void MakePile(Card[] cards, Vector2 position)
        {
            Random random = new Random();
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].cardFront.Move(Actions.ExpoMove, position + new Vector2(random.Next(-50 + 2* i,50-2*i),random.Next(-50+i,50-i)), ((float)i+1)/3);
                cards[i].cardFront.Rotate(Actions.ExpoMove, (float)(random.NextDouble() - .5)/2, ((float)i+1)/3);
            }
        }
    }
}