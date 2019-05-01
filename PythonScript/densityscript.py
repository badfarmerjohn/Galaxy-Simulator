from skimage import io
from skimage.color import rgb2gray
import numpy as np
import matplotlib.pyplot as plt

def blockshaped(arr, nrows, ncols):
    h, w = arr.shape
    return (arr.reshape(h // nrows, nrows, -1, ncols).swapaxes(1,2).reshape(-1, nrows, ncols))

original = io.imread('galaxy.jpg')
grayscale = rgb2gray(original)
print(grayscale.shape)
num_rows = 102
num_cols = 173
num_pixel_rows = grayscale.shape[0] // num_rows
num_pixel_cols = grayscale.shape[1] // num_cols
blocks = blockshaped(grayscale, num_pixel_rows, num_pixel_cols)
f = open("pixel_array.txt", "w")
curr_col = 0
im = np.zeros((num_rows * num_cols,))
for i in range(blocks.shape[0]):
	avg = np.mean(blocks[i])
	im[i] = avg
	f.write(str(avg) + " ")
	curr_col += 1
	if curr_col >= num_cols:
		f.write("\n")
		curr_col = 0

plt.imshow(im.reshape(num_rows, num_cols), cmap="gray")
plt.show()