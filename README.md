# Pixelator

A small program that automatically convert a high definition image into a pixelated one

# Controls

* load -> load an image from your files
* slider -> the upper slider controls  ​how many colors the auto palette ​creator should find; Pretty ​slow on a large number of colors
* save -> 3 options
    * save image : save the pixelated image
    * save large : save a larger version of the pixelated image without interpolation between pixels
    * save palette ​: save an image with the selected palette ​
* interpolation -> the interpolation method used in downscaling​the original image; my personal recommendation ​is either ​"low" or "nearest neighbor"
* color distance -> the method used for finding the closest color; it is used in the creation of the pallet and in the finding the color from the palette ​
* show all colors -> this option is used to show the downscaled version without choosing the colors from the palette ​
* size box -> the text box where the size of the final image can be selected
* add color -> a color dialog for adding a new color to the palette ​
* transform -> for the creations of the finale image that is affected by all the above options
* not buttons related -> there are auxiliary ​options for adding or removing ​colors from the palette ​; those options work on the 3 images: the original, the finale and the palette ​
    * left click -> add the selected color
    * right click -> remove the closest ​color

# Some resoults

those are images fresh out of the app

![Original](./images/big0.png)

Original (be gentle, it's an extreamly old drawing)

![200x120](./images/big2.png)

200x120

![100x60](./images/big1.png)

100x60

![Original](./images/icon0.jpg)

Original

![100x100](./images/icon2.png)

100x100

![100x100 pallete swap](./images/icon1.png)

100x100 pallete swap

![50x50](./images/icon3.png)

50x50

Have fun!