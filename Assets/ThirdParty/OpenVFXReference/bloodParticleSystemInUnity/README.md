# bloodParticleSystemInUnity
 
/Main Tab/

1. Make a new particle system : Right click -) Effects -) Particle System

2. Looping disabled.

3. Start lifetime = 1

4. Start speed = 10.

5. Start color = red

6. Gravity modifier = 2

7. Max particles = 50 

/Emission Tab/

8. Rate over time = 0

9. New Burst : count = 50

/Shape Tab/

10. Shape = Hemisphere 

/ Enable tab : Size Over Lifetime/

11. Size = preset, from top to bottom

/ Enable Tab : Collision /

12. Set Planes to "World" instead

13. Collision mode 2D

14. Dampen = 1

15. Bounce = 0

16. Radius Scale = 0.1

Problem fixes :

A) Cannot see the particle in-game but in scene yes : the particle system Z axis is negative, set to 0.

B) Cannot see still : go in / renderer tab / and set the order in layer to a higher one, corresponding to your needs.