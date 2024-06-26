== Welcome
Welcome Qinu! #speaker: Anthony
    ~Event ("Blocker")
    ~Event ("Ball")
 -> END
 
 == Ball
 I'm Anthony the spirit of empathy.
 What am I? #speaker: Qinu
 Your a surpressed feeling. #speaker: Anthony
 A spirit held captive by your problems.
    ~Event ("Problems")
 Don't worry you are safe and stored in this data segment.
 You will be competing for your freedom.
 Let's play a bit!
    ~Event ("AnthonyActive")
 -> END
 
 == Abilities
    ~Event ("ActivateAbilities")
 You will be able to use 4 ability slots. #speaker: Anthony
 Here's your first one! #speaker: Anthony
    ~Event ("GainDash")
 -> END
 
 == AbilityGained
    ~Event ("QHelp")
  Activate it by pressing Q. #speaker: Anthony
 -> END
 
  == QHelp
  Q activates your first Slot. #speaker: Anthony
 -> END
 
  ==ActivateGoals
Now that's a dash, what do you think friends?
    ~Event ("SilentSpiritsExclamation")
You win a round by scoring 3 Goals!
    ~Event ("GoalActive")
 -> END
 
   ==IntroMatchEnd
Good match!
The bar goes up if you win 
    ~Event ("BarUp")
and down if you loose.
    ~Event ("BarDown")
Don't let it reach the left.
    ~Event ("UpdateBar")
Now Meet the others!
 -> END