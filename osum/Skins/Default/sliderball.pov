// +w1024 +h1024 +ki0 +kf0.9 +kfi0 +kff9 +ua
#declare TEST = false;
#declare FINAL_BALL = true;
#declare FINAL_GLOSS = false;

#version 3.7

global_settings
{
    assumed_gamma 1
}

background
{
    color rgbt 1
}

camera
{
    orthographic
    location 1000*z
    look_at 0
    up 2*y
    right 2*x
}

// main light
light_source
{
    100*<1, 3, 6>
    color rgb 0.9
    parallel
    point_at 0
}

// filler
light_source
{
    100*<-3, 1, 1>
    color rgb 0.02
    parallel
    point_at 0
}

// straight-on filler
light_source
{
    100*<0, 0, 1>
    color rgb 0.08
    parallel
    point_at 0
}

// sliderbawl
sphere
{
    0, 59/64
    texture
    {
        pigment
        {
            radial
            frequency 4
            color_map
            {
                [0, 0.5 color rgb <0.005, 0.205, 1.000> color rgb <0.005, 0.205, 1.000>]
                [0.5, 1 color rgb <0.005, 0.005, 0.005> color rgb <0.005, 0.005, 0.005>]
            }
            rotate (clock+0.35)*-90*y
        }
        finish
        {
            #if (TEST | FINAL_BALL)
            diffuse 0.95 ambient 0.00
            #else
            diffuse 0 ambient 0
            #end
            #if (TEST | FINAL_GLOSS)
            reflection {0.02, 0.03}
            conserve_energy
            #end
        }
    }
}

// skysphere to provide shine
// (this isn't a sky_sphere because of the need for no_image)
sphere
{
    0, 1
    
    texture
    {
        pigment
        {
            gradient y
            color_map
            {
                [0, 0.5 color rgb 0 color rgb 0]
                [0.5 color rgb 1]
                [1 color rgb 0.2]
            }
            scale 2.2
            translate 1*y
            rotate 20*x
        }
        finish {diffuse 0 ambient 1}
    }
    
    scale 10000
    hollow
    no_image
}
