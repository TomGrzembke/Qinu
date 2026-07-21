== Welcome
Welcome Qinu! #speaker: Anthony
    ~Event ("Blocker")
 -> END
 
 == Ball
 What am I? #speaker: Qinu
 Your a suppressed feeling. #speaker: Anthony
 Don't worry about it for now.
 Is this.. an air hockey arena? #speaker: Qinu
 Exactly! Let's play a bit! #speaker: Anthony
    ~Event ("Ball")
    ~Event ("AnthonyActive")
    ~Event ("GoalActive")
 -> END
 
 == Abilities
  ~Event ("GoalActive")
  ~Event ("ActivateAbilities")
 You will be able to use 3 ability slots. #speaker: Anthony
 Here's your first one! #speaker: Anthony
    ~Event ("GainDash")
 -> END
 
 == AbilityGained
  Activate it with the left mouse button. #speaker: Anthony
         ~Event ("QHelp")
 -> END
 
  == QHelp
  Left mouse button activates your first Slot. #speaker: Anthony
 -> END
 
  ==ActivateGoals
   ~Event ("GoalActive")
Now that's a dash! #speaker: Anthony
Whoever scores 3 Goals first wins.
 -> END
 
   ==IntroMatchEnd
Good match! #speaker: Anthony
~Event ("BarUp")
The bar goes up if you win 
    ~Event ("BarDown")
and down if you loose.
Don't let it reach the left.
    ~Event ("UpdateBar")
Now Show 'em what you've got!
 -> END
 
 == StartArchive
 Welcome Qinu! #speaker: Anthony
    ~Event ("Blocker")
     I'm Anthony the spirit of empathy.
 What am I? #speaker: Qinu
 Your a surpressed feeling. #speaker: Anthony
 A spirit held captive by your problems.
    ~Event ("Problems")
 Don't worry you are safe and stored in this data segment.
 You will be competing for your freedom.
 Let's play a bit!
    ~Event ("Ball")
    ~Event ("AnthonyActive")
        ~Event ("ActivateAbilities")
 You will be able to use 3 ability slots. #speaker: Anthony
 Here's your first one! #speaker: Anthony
    ~Event ("GainDash")
     Activate it with the left mouse button. #speaker: Anthony
         ~Event ("QHelp")
 ->END