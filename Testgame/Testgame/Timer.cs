<<<<<<< HEAD
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Testgame
{
    class Timer : Drawable
    {
        public Tweener[] timer;
        
        public Timer(int x)
        {
            timer = new Tweener[x];
        }

        public void SetTimer(int timerNumber, float endTime, Tweener.EndHandler whenEnded)
        {
            timer[timerNumber] = new Tweener(0, 1, endTime, Actions.LinearMove);
            timer[timerNumber].Ended += whenEnded;
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteEffects spriteEffects)
        {
            return;
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < timer.Length; i++)
            {
                timer[i].Update(gameTime);
            }
        }
    }
}
=======
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Testgame
{
    class Timer : Drawable
    {
        public Tweener[] timer;
        
        public Timer(int x)
        {
            timer = new Tweener[x];
        }

        public void SetTimer(int timerNumber, float endTime, Tweener.EndHandler whenEnded)
        {
            timer[timerNumber] = new Tweener(0, 1, endTime, Actions.LinearMove);
            timer[timerNumber].Ended += whenEnded;
        }

        public override void Draw(SpriteBatch spriteBatch, SpriteEffects spriteEffects)
        {
            return;
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < timer.Length; i++)
            {
                timer[i].Update(gameTime);
            }
        }
    }
}
>>>>>>> f230a4a135d6a02843f2d595724e6f46d31ad5da
