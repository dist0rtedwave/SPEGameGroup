﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Testgame
{
    class ComputerPlayer : Player
    {
        float timeDelay;
        Timer moveDelay;
        int pileNumber;
        bool isLeftPile;
        CompState myState;

        Random random;

        MouseState oldState;


        public ComputerPlayer(String name, bool isPlayer1) : base(name, isPlayer1)
        {
            myState = CompState.normal;
            timeDelay = .40f;
            random = new Random();
        }

        public enum CompState
        {
            moving,
            normal,
        }

        public void Move(Pile[] Hand, Pile rgamestack, Pile lgamestack)
        {
            if (myState == CompState.moving) return;
            if (ExistAMove(Hand, rgamestack, lgamestack))
            {
                myState = CompState.moving;
                if (pileNumber != selector)
                {
                    int x = pileNumber - selector;
                    if (x >= 3 || (x < 0 && x >= -2))
                    {
                        moveDelay = new Timer(1);
                        moveDelay.SetTimer(0, timeDelay + ((float)random.NextDouble() - .5f) * timeDelay, delegate()
                        {
                            if (!isPlayer1) MoveSelectorLeft();
                            else MoveSelectorRight();
                            myState = CompState.normal;
                        });
                    }
                    else if (x <= -3 || (x > 0 && x < 3))
                    {
                        moveDelay = new Timer(1);
                        moveDelay.SetTimer(0, timeDelay + ((float)random.NextDouble() - .5f) * timeDelay, delegate()
                        {
                            if (!isPlayer1) MoveSelectorRight();
                            else MoveSelectorLeft();
                                myState = CompState.normal;
                        });
                    }
                }
                else if (pileNumber == selector)
                {
                    moveDelay = new Timer(1);
                    moveDelay.SetTimer(0, timeDelay + ((float)random.NextDouble() - .5f) * timeDelay, delegate()
                    { SelectCard(isLeftPile); myState = CompState.normal; });
                }
            }
        }

        public bool ExistAMove(Pile[] Hand, Pile rgamestack, Pile lgamestack)
        {
            bool moves = false;
            for (int i = 0; i < Hand.Length; i++)
            {
                if (!moves)
                {
                    if (Hand[i].Count() != 0)
                    {
                        int cv = Hand[i].Peek().cardValue;
                        int value1 = lgamestack.Peek().cardValue;
                        int value2 = rgamestack.Peek().cardValue;
                        if ((cv == 0 && value1 == 12) || (cv == 12 && value1 == 0) || (cv == value1 + 1 || cv == value1 - 1))
                        {
                            moves = true;
                            isLeftPile = true;
                            pileNumber = i;
                        }
                        if ((cv == 0 && value2 == 12) || (cv == 12 && value2 == 0) || (cv == value2 + 1 || cv == value2 - 1))
                        {
                            moves = true;
                            isLeftPile = false;
                            pileNumber = i;
                        }
                    }
                }
            }
            return moves;
        }


        public override void Update(Pile[] Hand, Pile rgamestack, Pile lgamestack, GameTime gameTime)

        {
            if (moveDelay != null) moveDelay.Update(gameTime);
            Move(Hand, rgamestack, lgamestack);
 	        base.Update(Hand, rgamestack, lgamestack, gameTime);
        }

        public override void Reset()
        {
            myState = CompState.normal;
            
            base.Reset();
        }
    }
}
