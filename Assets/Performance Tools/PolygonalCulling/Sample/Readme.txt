Dear Customer,

I made a couple of test scenes for You. "Showcase_baked" - a scene showing the final result Polygonal Culling.

You can do this yourself:

1) Open the scene "Showcase"

2) Click the "Tools / NGSTools / Polygonal Culling"

3) Adjust parametny. I did it like this:

     maxStack: 8;
     minTrianglesCount: 500;

     bakingType: mixed;
 
     callSize: 0.317;
     accuracy: 10;

     Cameras: in the hierarchy of "Player / Camera"

4) Look at the result.

I recommend You also try baking_type: "Standard_Occlusion"

Kindest regards,
Andrey.