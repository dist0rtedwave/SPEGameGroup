﻿
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;

namespace Speed
{
    class Speed : Screen
    {
        Pile yourStack;
        Pile opponentStack;
        Pile lSpitStack;
        Pile rSpitStack;
        Pile lGameStack;
        Pile rGameStack;
        Card[] deck;         // non-shuffled for reference
        Card[] cards;        // shuffled cards (actually played with)
        Pile[] yourCards;
        Pile[] opponentCards;
        KeyboardState oldState;
        GamePadState elderState;
        public gameState speedState { get; set; }
        Drawable oppSelector;
        Drawable yourSelector;
        public readonly Player you;
        public readonly Player opp;
        SpriteFont _font;
        public bool isHalted { get; set; }
        public bool isShaking { get; set; }
        Text yourName;
        Text oppName;
        Text yourScore;
        Text oppScore;
        Text time1;
        Text time2;
        Random random;
        List<Texture2D> textures;
        ParticleEngine engine;
        int[] cardcounter;            // keep track of generated cards;
        int numberOfDecks = 1;        // number of decks to be used at a time

        public readonly gameType myType;
        public int winningscore { get; set; }
        Timer gameTimer;
        public int gameLength { get; set; }
        SoundEffect shuffle;
        SoundEffect playcard;
        SoundEffectInstance shuffleinstance;
        bool soundOn;
        bool powerUpOn;
        bool vibrateOn;
        bool ableToReBegin;
        bool ableToReBegin2;
        public int level { get; set; }
        public delegate void EndGameMethod();
        public event EndGameMethod YouWon;
        public event EndGameMethod YouLost;
        public event EndGameMethod YouTie;
        Timer delayTimer;
        Text levelss;
        PowerUp freeze;

        // initializes lots of variables
        public Speed(Card[] deckOfCards, Drawable background, Texture2D selector, SpriteFont font, 
            Player bottom, Player top, List<Texture2D> particles, gameType gameType, SoundEffect shuffling, 
            SoundEffect playingcard, SoundEffectInstance shuffinstance, bool isSoundOn, bool isPowerUpOn, bool isVibrateOn, PowerUp powerup):base(background)
        {
            myType = gameType;
            cardcounter = new int[52];
            random = new Random();
            _font = font;
            shuffle = shuffling;
            playcard = playingcard;
            shuffleinstance = shuffinstance;
            isHalted = false;
            isShaking = false;
            textures = particles;
            soundOn = isSoundOn;
            powerUpOn = isPowerUpOn;
            vibrateOn = isVibrateOn;
            ableToReBegin = true;
            ableToReBegin2 = true;
            freeze = powerup;
            freeze.isSeeable = false;
            base.Add(freeze);
            freeze.WhenPlayed(delegate(Player player)
            {
                player.Freeze(); 
                Timer timer = new Timer(1); base.Add(timer); timer.SetTimer(0, 4, delegate()
                {
                    player.UnFreeze();
                }); 
                freeze.Fade(4); 
                if (player.isPlayer1) freeze.Move(Actions.ExpoMove, yourSelector.attributes.position, 1);
                else freeze.Move(Actions.ExpoMove, oppSelector.attributes.position, 1);
            });

            you = bottom;
            opp = top;
            you.SelectedCardLeft += delegate(){ PlayCard(you, you.selector, lGameStack);};
            you.SelectedCardRight += delegate() { PlayCard(you, you.selector, rGameStack); };
            opp.SelectedCardLeft += delegate() { PlayCard(opp, opp.selector, lGameStack); };
            opp.SelectedCardRight += delegate() { PlayCard(opp, opp.selector, rGameStack); };
            you.score = 0;
            opp.score = 0;

            deck = new Card[deckOfCards.Length];
            cards = new Card[deckOfCards.Length];
            for (int i = 0; i < deckOfCards.Length; i++)
            {
                deck[i] = deckOfCards[i];
                cards[i] = deckOfCards[i];
            }

            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].attributes.position = new Vector2(-100, 100);
                cards[i].isFaceUp = false;
                cards[i].isSeeable = true;
                cards[i].attributes.color = Color.White;
                cards[i].attributes.rotation = 0;
                cards[i].ClearTweeners();
                cards[i].attributes.height = 180;
                cards[i].attributes.width = 130;
                deck[i].attributes.position = new Vector2(-100, 100);
                deck[i].isFaceUp = false;
                deck[i].isSeeable = true;
                deck[i].attributes.color = Color.White;
                deck[i].attributes.rotation = 0;
                deck[i].ClearTweeners();
                deck[i].attributes.height = 180;
                deck[i].attributes.width = 130;
            }
            yourStack = new Pile(new Vector2(897, 675));
            opponentStack = new Pile(new Vector2(127, 125));
            lSpitStack = new Pile(new Vector2(217, 400));
            rSpitStack = new Pile(new Vector2(807, 400));
            lGameStack = new Pile(new Vector2(413, 400));
            rGameStack = new Pile(new Vector2(610, 400));
            yourCards = new Pile[5];
            opponentCards = new Pile[5];

            for (int i = 0; i < yourCards.Length; i++)
            {
                opponentCards[i] = new Pile(new Vector2(opponentStack.position.X + (i + 1) * 154, opponentStack.position.Y));
                yourCards[i] = new Pile(new Vector2(yourStack.position.X - (i + 1) * 154, yourStack.position.Y));
            }

            yourName = new Text(you.playerName + " -  ", _font);
            yourName.height = 100;
            yourName.attributes.position = new Vector2((yourCards[3].position.X + yourCards[4].position.X)/2 - 20, (yourCards[3].position.Y + lGameStack.position.Y) / 2);
            yourName.isSeeable = true;
            yourName.attributes.color = Color.LightSkyBlue;
            yourName.attributes.depth = .02f;
            yourName.scale = new Vector2(.15f, .15f);
            
            oppName = new Text("  - " + opp.playerName, _font);
            oppName.height = 100;
            oppName.attributes.position = new Vector2((opponentCards[3].position.X + opponentCards[4].position.X)/2 + 20, (opponentCards[3].position.Y + lGameStack.position.Y) / 2);
            oppName.isSeeable = true;
            oppName.attributes.color = Color.Red;
            oppName.attributes.depth = .02f;
            oppName.scale = new Vector2(.15f, .15f);
            
            yourScore = new Text(you.score.ToString(), _font);
            yourScore.height = 100;
            yourScore.attributes.position = new Vector2(yourName.attributes.position.X + yourName.width/2 + 10, yourName.attributes.position.Y);
            yourScore.isSeeable = true;
            yourScore.attributes.color = Color.LightSkyBlue;
            yourScore.attributes.depth = .02f;
            yourScore.scale = new Vector2(.15f, .15f);

            oppScore = new Text(opp.score.ToString(), _font);
            oppScore.height = 100;
            oppScore.attributes.position = new Vector2(oppName.attributes.position.X - oppName.width/2 - 10, oppName.attributes.position.Y);
            oppScore.isSeeable = true;
            oppScore.attributes.color = Color.Red;
            oppScore.attributes.depth = .02f;
            oppScore.scale = new Vector2(.15f, .15f);
            
            for (int i = 0; i < cards.Length; i++)
            {
                base.Add(cards[i]);
            }

            oppSelector = new Drawable()
            {
                attributes = new Attributes()
                {
                    texture = selector,
                    position = new Vector2(-100, -100),
                    color = Color.Red,
                    height = 190,
                    width = 140,
                    depth = .11f,
                    rotation = 0
                }
            };

            yourSelector = new Drawable()
            {
                attributes = new Attributes()
                {
                    texture = selector,
                    position = new Vector2(-100, -100),
                    color = Color.LightSkyBlue,
                    height = 190,
                    width = 140,
                    depth = .11f,
                    rotation = 0
                }
            };

            base.Add(oppSelector);
            base.Add(yourSelector);

            if (myType == gameType.Marathon)
            {
                winningscore = 30;
            }

            //if (myType == gameType.Timed)
            //{
                gameLength = 120;
                delayTimer = new Timer(1);
                delayTimer.SetTimer(0, 4, delegate()
                {
                    if (speedState == gameState.GamePlay && lSpitStack.Count() != 0)
                    {
                        speedState = gameState.ReBeginning;
                        Text delay = new Text("Delay", font)
                        {
                            attributes = new Attributes()
                                {
                                    color = Color.Yellow,
                                    position = new Vector2(512, 400),
                                    depth = .001f,
                                },
                            height = 400,
                        };
                        base.Add(delay);
                        delay.Fade(2);
                        delay.WhenDoneFading(delegate() { BeginGame(); });
                    }
                });                   
            //}
        }

        public enum gameType
        {
            Normal,               // First to clear hand
            Marathon,             // First to 30
            Timed,                // Most in 2 minutes
            Levels,               // Levels
        }


        // different gamestates
        public enum gameState
        {
            Dealing,
            AskBegin,
            Beginning,
            GamePlay,
            ReBeginning,
            PlayingCard,
            Winner,
            PlayAgain,
        }

        // deals cards to their piles, says to press enter to start
        public void Deal()
        {
            speedState = gameState.Dealing;
            if (myType == gameType.Levels)
            {
                Text levelText = new Text("Level " + level, _font)
                {
                    height = 200,
                    attributes = new Attributes()
                    {
                        color = Color.Yellow,
                        depth = .5f,
                        position = new Vector2(512, 400),
                    },
                };
                base.Add(levelText);
                levelText.Fade(4);
            }
            Shuffle(cards);
            for (int i = 0; i < cards.Length; i++)
            {
                cards[i].isFaceUp = false;
                if (i % 4 == 3 && i < 20) cards[i].toPile(rSpitStack, (float)i / 13);
                else if (i % 4 == 2 && i < 20) cards[i].toPile(lSpitStack, (float)i / 13);
                else if (i % 2 == 1) cards[i].toPile(yourStack, (float)i / 13);
                else if (i % 2 == 0) cards[i].toPile(opponentStack, (float)i / 13);
            }
            Text begin = new Text("Press Enter to Start", _font)
            {
                attributes = new Attributes()
                {
                    color = Color.Yellow,
                    position = new Vector2(512, 400),
                    rotation = 0,
                    depth = .5f
                },
                scale = new Vector2(.1f,.1f)
            };
            cards[51].WhenDoneMoving( delegate()
            {
                base.Add(begin);
                speedState = gameState.AskBegin;
            });
        }

        // 
        public void Begin()
        {
            base.RemoveLast();
            if (myType == gameType.Timed)
            {
                //base.Add(time1);
                base.Add(time2);
            }

            if (myType == gameType.Levels)
            {
                levelss = new Text("Level " + level, _font)
                {
                    height = 100,
                    attributes = new Attributes()
                    {
                        color = Color.Black,
                        position = new Vector2(rSpitStack.position.X + 50, yourName.attributes.position.Y),
                        depth = .01f
                    },
                    scale = new Vector2(.15f, .15f),
                };
                base.Add(levelss);

            }
                base.Add(yourName);
                base.Add(oppName);
                base.Add(yourScore);
                base.Add(oppScore);
            speedState = gameState.Beginning;
            for (int i = 0; i < 5; i++)
            {
                DrawCard(yourStack, yourCards[i], i * .675f);
                DrawCard(opponentStack, opponentCards[i], i * .675f);
            }

            Timer stopwatch = new Timer(3);
            base.Add(stopwatch);
            Text three = new Text("3", _font) { attributes = new Attributes() { color = Color.Yellow, position = new Vector2(512, 400) } };
            base.Add(three);
            three.Fade(1);
           
            stopwatch.SetTimer(0, 1, delegate()
            {
                Text two = new Text("2", _font) { attributes = new Attributes() { color = Color.Yellow, position = new Vector2(512, 400) } };
                base.Add(two);
                two.Fade(1);
                
            });
            stopwatch.SetTimer(1, 2, delegate()
            {
                Text one = new Text("1", _font) { attributes = new Attributes() { color = Color.Yellow, position = new Vector2(512, 400) } };
                base.Add(one);
                one.Fade(1);
                
            });
            stopwatch.SetTimer(2, 3, delegate()
            {
                Text title = new Text("SPEED!", _font) { attributes = new Attributes() { color = Color.Yellow, position = new Vector2(512, 400) } };
                base.Add(title);
                title.Fade(.5f);
                title.WhenDoneFading(new Tweener.EndHandler(BeginGame));
                if(myType == gameType.Timed) title.WhenDoneFading(delegate() { base.Add(gameTimer); });
            });
        }

        // draws card to different piles
        public void DrawCard(Pile drawPile, Pile destinationPile, float delay)
        {
            if (destinationPile.drawnTo) return;
            ableToReBegin = false;
            destinationPile.drawnTo = true;
            Card temp = null;
            switch (myType)
            {
                case gameType.Levels:
                case gameType.Normal:
                    temp = drawPile.Take();
                    break;
                case gameType.Marathon:
                case gameType.Timed:


                    temp = getRandomCard(drawPile);
                    base.Add(temp);
                    break;
            }
            temp.Flip(true, delay);
            temp.toPile(destinationPile, delay);
            if (destinationPile.hasPowerUp) destinationPile.RemovePowerUp();
            temp.WhenDoneMoving(delegate() { destinationPile.drawnTo = false; Timer drawTimer = new Timer(1);
            base.Add(drawTimer); drawTimer.SetTimer(0, .5f, delegate() { ableToReBegin = true; });
            
                if ((destinationPile == lGameStack || destinationPile == rGameStack) && !destinationPile.hasPowerUp) SelectPowerUp(destinationPile);
            
            });
        }

        public enum PowerUps
        {
            freeze
        }

        //
        private void SelectPowerUp(Pile pile)
        {
            if (powerUpOn)
            {
                double r = random.NextDouble();
                if (r < .05)
                {
                    int x = random.Next(0, (int)PowerUps.freeze);

                    PowerUp newPower = new PowerUp(freeze.engine.attributes.color, freeze.engine.textures, powerUpOn)
                    {
                        attributes = new Attributes()
                        {
                            texture = freeze.attributes.texture,
                            position = new Vector2(100, 100),
                            color = Color.White
                        }
                    };
                    newPower.isSeeable = false;
                    newPower.WhenPlayed(delegate(Player player)
                {
                    player.Freeze();
                    newPower.Fade(4);
                    if (player.isPlayer1) newPower.Move(Actions.ExpoMove, yourSelector.attributes.position, 1);
                    else newPower.Move(Actions.ExpoMove, oppSelector.attributes.position, 1);
                });
                    base.Add(newPower);
                    pile.GivePowerUp(newPower);
                }
            }
        }

        // 
        private Card getRandomCard(Pile DrawPile)
        {
            bool foundint = false;
            int s = 0;
            while (!foundint)
            {
                int x = random.Next(0, 52);
                if (cardcounter[x] < numberOfDecks) foundint = true;
                s = x;
            }
            cardcounter[s]++;
            bool full = true;
            for (int i = 0; i < cardcounter.Length; i++)
            {
                if (cardcounter[i] < numberOfDecks) full = false;
            }

            if (full) cardcounter = new int[52];
            return new Card(s, deck[s].cardFront, deck[s].cardBack, DrawPile.position, false);
        }

        // update method based on different gamestates
        public override void Update(GameTime gameTime)
        {
            if (base.isPaused) return;
            if (isHalted) return;
            if (speedState == gameState.GamePlay) delayTimer.Update(gameTime);
            if (myType == gameType.Timed)
            {
                //time1.changeContent(gameTimer.getCountDown(0));
                time2.changeContent(gameTimer.getCountDown(0));
                //if (speedState == gameState.GamePlay) delayTimer.Update(gameTime);
                if (speedState == gameState.ReBeginning && gameTimer.getTimeLeft(0) <= 20) gameTimer.isPaused = true;
                else gameTimer.isPaused = false;
            }

            switch (speedState)
            {
                case gameState.Dealing:
                    if ((shuffleinstance.State == SoundState.Stopped) && soundOn)
                        shuffleinstance.Play();
                    break;
                case gameState.AskBegin:
                case gameState.Beginning:
                    DoNothing();
                    break;
                case gameState.GamePlay:
                    yourScore.changeContent(you.score.ToString());
                    oppScore.changeContent(opp.score.ToString());
                    if (ExistWinner()) Winner(DetermineWinner());
                    else if (!ExistMoves()) ReBegin();
                    you.Update(yourCards,rGameStack, lGameStack, gameTime);
                    opp.Update(opponentCards, rGameStack, lGameStack, gameTime);              
                    yourSelector.attributes.position = yourCards[you.selector].position;
                    oppSelector.attributes.position = opponentCards[opp.selector].position;                  
                    break;             
                case gameState.ReBeginning:
                case gameState.PlayingCard:
                    you.Update(yourCards, rGameStack, lGameStack, gameTime);
                    opp.Update(opponentCards, rGameStack, lGameStack, gameTime);
                    yourSelector.attributes.position = yourCards[you.selector].position;
                    oppSelector.attributes.position = opponentCards[opp.selector].position;
                    break;
                case gameState.Winner:
                    break;
                case gameState.PlayAgain:
                    break;
            }
            //bool moveExists = ComputerPlayer.FindPileNumber(opponentCards, lGameStack, rGameStack);
            KeyUpdate(gameTime);
            base.Update(gameTime);
        }

        // updates keys, if enter is pressed while game is about to start, starts it
        protected void KeyUpdate(GameTime gameTime)
        {
            KeyboardState newState = Keyboard.GetState();
            GamePadState youngState = GamePad.GetState(PlayerIndex.One);
            if (speedState == gameState.AskBegin)
            {
                if (newState.IsKeyDown(Keys.Enter))
                {
                    if (!oldState.IsKeyDown(Keys.Enter))
                    {
                        Begin();
                    }
                }

                if (youngState.IsButtonDown(Buttons.A))
                {
                    if (!elderState.IsButtonDown(Buttons.A))
                    {
                        Begin();
                    }
                }
            }
            oldState = newState;
            elderState = youngState;
        }

        // resumes the game
        public override void Resume()
        {
            isHalted = false;
            base.Resume();
        }

        // turns game on
        public override void TurnOn()
        {
            if (myType == gameType.Timed)
            {
                gameTimer = new Timer(1);
                gameTimer.SetTimer(0, gameLength, delegate() { Winner(DetermineWinner()); ableToReBegin = false; ableToReBegin2 = false; /*time1.Fade(4);*/ time2.Fade(4); speedState = gameState.Winner; });

                /*time1 = new Text(gameTimer.getCountDown(0), _font);
                time1.height = 100;
                time1.attributes.position = new Vector2(yourName.attributes.position.X, oppName.attributes.position.Y);
                time1.isSeeable = true;
                time1.attributes.color = Color.Black;
                time1.attributes.depth = .02f;
                time1.scale = new Vector2(.15f, .15f);*/

                time2 = new Text(gameTimer.getCountDown(0), _font);
                time2.height = 100;
                time2.attributes.position = new Vector2(oppName.attributes.position.X, yourName.attributes.position.Y);
                time2.isSeeable = true;
                time2.attributes.color = Color.Black;
                time2.attributes.depth = .02f;
                time2.scale = new Vector2(.12f, .12f);
            }
            Deal();
            base.TurnOn();
        }

        // halts game
        public void Halt()
        {
            isHalted = true;
        }

        // do nothing
        public void DoNothing()
        {
            return;
        }

        // starts the game
        public void BeginGame()
        {         
            Timer timer = new Timer(1);
            base.Add(timer);
            timer.SetTimer(0, .4f, delegate() { speedState = gameState.GamePlay; ableToReBegin = true; });
            DrawCard(lSpitStack, lGameStack, 0f);
            DrawCard(rSpitStack, rGameStack, 0f);
            you.TurnOn();
            opp.TurnOn();            
        }

        // plays cards from selected piles to destination piles
        public void PlayCard(Player player, int selectedPile, Pile destinationPile)
        {
            if (speedState != gameState.GamePlay) return;
            Pile fromPile;
            if (player.isPlayer1) fromPile = yourCards[player.selector];
            else fromPile = opponentCards[player.selector];
            if (fromPile.Count() == 0)
            {
                return;
            }
            if (fromPile.drawnTo) return;
            speedState = gameState.PlayingCard;
            Card c = fromPile.Peek();
            int cv = c.cardValue;
            int value = destinationPile.Peek().cardValue;
            if ((cv == 0 && value == 12) || (cv == 12 && value == 0) || (cv == value + 1 || cv == value - 1))
            {
                //if(myType == gameType.Timed) 
                delayTimer.ResetTimer(0);
                Card m = fromPile.Take();
                m.toPile(destinationPile);
                if (soundOn)
                {
                    playcard.Play();
                }
                if (vibrateOn)
                {
                    GamePad.SetVibration(PlayerIndex.One, 1.0f, 1.0f);
                }
                m.Rotate(Actions.ExpoMove, (float)(random.NextDouble() - .5) / 2, .3f);
                m.WhenDoneMoving(delegate()
                {
                    m.Move(Actions.LinearMove, m.attributes.position + new Vector2(random.Next(-5,5), random.Next(-5, 5)), 3f); 
                    player.score++;
                    if (powerUpOn)
                    {
                        if (destinationPile.hasPowerUp)
                        {
                            Player victim;
                            if (player.isPlayer1) victim = opp;
                            else victim = you;
                            destinationPile.PlayPowerUp(victim);
                        }
                    }
                    speedState = gameState.GamePlay;
                    if (player.isPlayer1)
                    {
                        if (yourStack.Count() != 0)
                        {
                            if (fromPile.Count() == 0)
                            {
                                DrawCard(yourStack, fromPile, 0f);
                            }
                        }
                    }
                    else
                    {
                        if (opponentStack.Count() != 0)
                        {
                            if (fromPile.Count() == 0)
                            {
                                DrawCard(opponentStack, fromPile, 0f);
                            }
                        }
                    }
                    Shake();
                    SelectPowerUp(destinationPile);
                    ParticleEngine smoke = new ParticleEngine(textures, destinationPile.position,new Vector2(300,300), m.attributes.depth, .3f, Color.WhiteSmoke);
                    base.Add(smoke);
                });
            }
            else
            {
                speedState = gameState.GamePlay;
                c.attributes.color = Color.Red;
                Timer timer = new Timer(1);
                base.Add(timer);
                timer.SetTimer(0, 1, delegate() { c.attributes.color = Color.White; player.RemovePenalty(); });
                player.Penalize();
            }
        }
        
        // determines if moves are still possible, returns boolean
        public bool ExistMoves()
        {
            if (speedState == gameState.Winner) return true;
            if (speedState == gameState.PlayAgain) return true;
            if (!ableToReBegin || !ableToReBegin2) return true;
            bool oppMoves = false;
            for (int i = 0; i < opponentCards.Length; i++)
            {
                if (opponentCards[i].Count() != 0)
                {
                    int cv = opponentCards[i].Peek().cardValue;
                    int value1 = lGameStack.Peek().cardValue;
                    int value2 = rGameStack.Peek().cardValue;
                    if ((cv == 0 && value1 == 12) || (cv == 12 && value1 == 0) || (cv == value1 + 1 || cv == value1 - 1)) oppMoves = true;
                    if ((cv == 0 && value2 == 12) || (cv == 12 && value2 == 0) || (cv == value2 + 1 || cv == value2 - 1)) oppMoves = true;
                }
            }
            bool yourMoves = false;
            for (int i = 0; i < yourCards.Length; i++)
            {
                if (yourCards[i].Count() != 0)
                {
                    int cv = yourCards[i].Peek().cardValue;
                    int value1 = lGameStack.Peek().cardValue;
                    int value2 = rGameStack.Peek().cardValue;
                    if ((cv == 0 && value1 == 12) || (cv == 12 && value1 == 0) || (cv == value1 + 1 || cv == value1 - 1)) yourMoves = true;
                    if ((cv == 0 && value2 == 12) || (cv == 12 && value2 == 0) || (cv == value2 + 1 || cv == value2 - 1)) yourMoves = true;
                }
            }
            return yourMoves || oppMoves;
        }

        // adds cards to game stacks if no cards are playable
        public void ReBegin()
        {
            if (!ableToReBegin || !ableToReBegin2) return;
            if (speedState == gameState.Winner) return;
            if (speedState == gameState.PlayAgain) return;
            ableToReBegin = false;
            speedState = gameState.ReBeginning;
                Timer watch = new Timer(1);
                base.Add(watch);
                watch.SetTimer(0, 1, delegate()
                {
                    Text nomove = new Text("No Moves", _font)
                    {
                        attributes = new Attributes() { color = Color.Yellow, position = new Vector2(512, 400) },
                        scale = new Vector2(.5f, .5f)
                    };
                    base.Add(nomove);
                    nomove.Move(Actions.LinearMove, nomove.attributes.position, 1);
                    nomove.WhenDoneMoving(delegate() { nomove.isSeeable = false; });
                });
            if (lSpitStack.Count() != 0)
            {
                Timer stopwatch = new Timer(5);
                base.Add(stopwatch);

                stopwatch.SetTimer(0, 3, delegate()
                {
                    Text two = new Text("2", _font) { attributes = new Attributes() { color = Color.Yellow, position = new Vector2(512, 400) } };
                    base.Add(two);
                    two.Fade(1);
                });
                stopwatch.SetTimer(1, 4, delegate()
                {
                    Text one = new Text("1", _font) { attributes = new Attributes() { color = Color.Yellow, position = new Vector2(512, 400) } };
                    base.Add(one);
                    one.Fade(1);
                });
                stopwatch.SetTimer(2, 5, delegate()
                {
                    Text title = new Text("SPEED!", _font) { attributes = new Attributes() { color = Color.Yellow, position = new Vector2(512, 400) } };
                    base.Add(title);
                    title.Fade(.5f);
                    title.WhenDoneFading(new Tweener.EndHandler(BeginGame));
                });
                stopwatch.SetTimer(3, 2, delegate()
                {
                    Text three = new Text("3", _font) { attributes = new Attributes() { color = Color.Yellow, position = new Vector2(512, 400) } };
                    base.Add(three);
                    three.Fade(1);
                });
            }

            else Winner(DetermineWinner());
        }

        // determines if there is a winner
        public bool ExistWinner()
        {
            if (myType == gameType.Normal || myType == gameType.Levels)
            {
                bool youWinner = true;
                for (int i = 0; i < yourCards.Length; i++)
                {
                    if (yourCards[i].Count() != 0) youWinner = false;
                }
                if (yourStack.Count() != 0) youWinner = false;

                bool oppWinner = true;
                for (int i = 0; i < opponentCards.Length; i++)
                {
                    if (opponentCards[i].Count() != 0) oppWinner = false;
                }
                if (opponentStack.Count() != 0) oppWinner = false;
                return youWinner || oppWinner;
            }

            if (myType == gameType.Marathon)
            {
                return (you.score == winningscore) || (opp.score == winningscore);
            }

            else return false;
        }

        // returns the player who won based on their score
        public Player DetermineWinner()
        {
            if (you.score > opp.score) return you;
            if (opp.score > you.score) return opp;
            else return null;
        }

        // does actions if player is a winner
        public void Winner(Player winner)
        {
            speedState = gameState.Winner;
            yourName.Fade(4);
            oppName.Fade(4);
            yourScore.Fade(4);
            oppScore.Fade(4);
            if (myType == gameType.Levels)
            {
                levelss.Fade(4);
            }
            for (int i = 0; i < yourCards.Length; i++)
            {
                if(yourCards[i].Count() != 0) yourCards[i].Take().Fade(4);
                if(opponentCards[i].Count() != 0) opponentCards[i].Take().Fade(4);
            }
            while (yourStack.Count() != 0)
            {
                yourStack.Take().Fade(4);
            }
            while (opponentStack.Count() != 0)
            {
                opponentStack.Take().Fade(4);
            }
            if (winner == null) Tie();
            else if (winner.isPlayer1) YourAWinner();
            else OppAWinner();
        }

        // if bottom player wins, displays certain text & fades some drawables& adds endgame actions
        public void YourAWinner()
        {
            speedState = gameState.Winner;
            
            Text winner = new Text("Winner!!!", _font)
            {
                attributes = new Attributes()
                {
                    color = Color.LightSkyBlue,
                    position = new Vector2(512, 650),
                    //rotation = -.2f,
                    depth = .1f,
                },
                scale = new Vector2(.8f,.8f)
            };
            Text loser = new Text("Scrub!!!", _font)
            {
                attributes = new Attributes()
                {
                    color = Color.DarkRed,
                    position = new Vector2(512, 180),
                    //rotation = -.2f,
                    depth = .1f
                },
                scale = new Vector2(.8f,.8f)
            };
            oppSelector.Fade(2);
            yourSelector.Move(Actions.ExpoMove, new Vector2(512, 400), 2);
            yourSelector.Fade(2);
            yourSelector.WhenDoneMoving( delegate()
            {
                base.Add(winner);
                base.Add(loser);    
            });
            yourSelector.Scale(Actions.LinearMove, yourSelector.attributes.scale * 2, 2);
            Timer endGame = new Timer(2);
            base.Add(endGame);
            endGame.SetTimer(0, 6, delegate()
            {
                yourScore.attributes.color = Color.LightSkyBlue;
                yourScore.isSeeable = true;
                yourScore.attributes.position = new Vector2(400, 675);
                yourScore.height = 150;
                oppScore.attributes.color = Color.Red;
                oppScore.isSeeable = true;
                oppScore.attributes.position = new Vector2(624, 675);
                oppScore.height = 150;
                Text dash = new Text(" - ", _font);
                dash.attributes = new Attributes() { color = Color.Black, position = new Vector2(512, 675) };
                dash.isSeeable = true;
                dash.height = 150;
                base.Add(dash);
                if (myType == gameType.Levels) YouWon();
                else Reset(); });
            endGame.SetTimer(1, 4, delegate() { winner.Fade(2); loser.Fade(2); });
        }

        // if top player wins, displays certain text & fades some drawables& adds endgame actions
        public void OppAWinner()
        {
            speedState = gameState.Winner;
            Text winner = new Text("Winner!!!", _font)
            {
                attributes = new Attributes()
                {
                    color = Color.Red,
                    position = new Vector2(512, 180),
                    //rotation = -.2f,
                    depth = .1f
                },
                scale = new Vector2(.8f,.8f)
            };
            Text loser = new Text("Scrub!!!", _font)
            {
                attributes = new Attributes()
                {
                    color = Color.DarkBlue,
                    position = new Vector2(512, 650),
                    //rotation = -.2f,
                    depth = .1f
                },
                scale = new Vector2(.8f,.8f)
            };
            yourSelector.Fade(2);
            oppSelector.Fade(2);
            oppSelector.Move(Actions.ExpoMove, new Vector2(512, 400), 2);
            oppSelector.WhenDoneMoving( delegate()
            {
                base.Add(winner);
                base.Add(loser);
            });
            oppSelector.Scale(Actions.LinearMove, yourSelector.attributes.scale * 2, 2);
            Timer endGame = new Timer(2);
            base.Add(endGame);
            endGame.SetTimer(0, 6, delegate()
            {
                yourScore.attributes.color = Color.LightSkyBlue;
                yourScore.isSeeable = true;
                yourScore.attributes.position = new Vector2(624, 675);
                yourScore.height = 150;
                oppScore.attributes.color = Color.Red;
                oppScore.isSeeable = true;
                oppScore.attributes.position = new Vector2(400, 675);
                oppScore.height = 150;
                Text dash = new Text(" - ", _font);
                dash.attributes = new Attributes() { color = Color.Black, position = new Vector2(512, 675) };
                dash.isSeeable = true;
                dash.height = 150;
                base.Add(dash);
                if (myType == gameType.Levels) YouLost();
                else Reset(); });
            endGame.SetTimer(1, 4, delegate() { winner.Fade(2); loser.Fade(2); });
        }

        // if game ends in a tie, displays certain text & fades some drawables& adds endgame actions
        public void Tie()
        {
            speedState = gameState.Winner;
            Text tieTop = new Text("It's a tie!", _font)
            {
                attributes = new Attributes()
                {
                    position = new Vector2(512, 200),
                    color = Color.RoyalBlue,
                    //rotation = -.2f,
                    depth = .1f
                },
                scale = new Vector2(.8f,.8f)
            };

            Text tieMiddle = new Text("You're both", _font)
            {
                attributes = new Attributes()
                {
                    position = new Vector2(512, 400),
                    color = Color.Red,
                    //rotation = -.2f,
                    depth = .1f
                },
                scale = new Vector2(.7f,.7f)
            };

            Text tieBottom = new Text("SCRUBS!!!", _font)
            {
                attributes = new Attributes()
                {
                    position = new Vector2(512, 600),
                    color = Color.Red,
                    //rotation = -.2f,
                    depth = .1f
                },
                scale = new Vector2(.7f,.7f)
            };
            yourSelector.Fade(2);
            yourSelector.Move(Actions.ExpoMove, new Vector2(512, 400), 2);
            yourSelector.Scale(Actions.LinearMove, yourSelector.attributes.scale * 2, 2);
            oppSelector.Fade(2);
            oppSelector.Move(Actions.ExpoMove, new Vector2(512, 400), 2);
            oppSelector.WhenDoneMoving( delegate()
            {
                base.Add(tieTop);
                if (myType == gameType.Levels) YouTie();
                base.Add(tieMiddle);
                base.Add(tieBottom);
            });
            oppSelector.Scale(Actions.LinearMove, yourSelector.attributes.scale * 2, 2);
            Timer endGame = new Timer(2);
            base.Add(endGame);
            endGame.SetTimer(0, 6, delegate()
            {
                yourScore.attributes.color = Color.LightSkyBlue;
                yourScore.isSeeable = true;
                yourScore.attributes.position = new Vector2(400, 675);
                yourScore.height = 150;
                oppScore.attributes.color = Color.Red;
                oppScore.isSeeable = true;
                oppScore.attributes.position = new Vector2(624, 675);
                oppScore.height = 150;
                Text dash = new Text(" - ", _font);
                dash.attributes = new Attributes() { color = Color.Black, position = new Vector2(512, 675) };
                dash.isSeeable = true;
                dash.height = 150;
                base.Add(dash);
                Reset();});
            endGame.SetTimer(1, 4, delegate() { tieTop.Fade(2); tieMiddle.Fade(2); tieBottom.Fade(2); });
        }

        // resets gamestate
        public void Reset()
        {
            speedState = gameState.PlayAgain;
        }

        // shakes screen
        public void Shake()
        {
            Timer timer = new Timer(1);
            base.Add(timer);
            isShaking = true;
            timer.SetTimer(0, .2f, delegate() { isShaking = false; });
        }

        // shuffles cards 
        public static void Shuffle(Card[] cards)
        {         
            Random random = new Random();
            int N = cards.Length;
            for (int i = 0; i < N; i++)
            {
                int r = i + (int)(random.NextDouble() * (N - i));
                Card temp = cards[r];
                cards[r] = cards[i];
                cards[i] = temp;
            };
        }
    }
}