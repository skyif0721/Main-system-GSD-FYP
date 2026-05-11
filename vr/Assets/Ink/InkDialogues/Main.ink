EXTERNAL OpenShop()
VAR sentence = 0

===npc===
Hi. I am cap_sure.
+ [chat]
    { sentence == 0:
    "Hope you survive!"
    ~ sentence += 1
  - else:
    "Do you want to buy something?"
    ~ sentence -= 1
}
    -> npc
+ [buy something]
    see what you need
    ~ OpenShop()
    -> END
* [nothing]
    -> END
-->END
