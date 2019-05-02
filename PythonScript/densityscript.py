from skimage import io
from skimage.color import rgb2gray
import numpy as np
import matplotlib.pyplot as plt
import sys

# def blockshaped(arr, nrows, ncols):
#     h, w = arr.shape
#     return arr.reshape(h // nrows, nrows, -1, ncols).swapaxes(1,2).reshape(-1, nrows, ncols)

def blockshaped(arr, num_tile_rows, num_tile_cols):
    h, w = arr.shape
    return arr.reshape(num_tile_rows, h // num_tile_rows, num_tile_cols, -1).transpose((0, 2, 1, 3)).reshape(num_tile_rows, num_tile_cols, -1)

def blockshaped_color(arr, num_tile_rows, num_tile_cols):
    h, w = arr.shape
    return arr.reshape(num_tile_rows, h // num_tile_rows, num_tile_cols, -1, 3).transpose((0, 2, 1, 3)).reshape(num_tile_rows, num_tile_cols, -1, 3)


if __name__ == "__main__":
	if len(sys.argv) < 2:
		print("Usage: python {} [image_file]".format(sys.argv[0]))
		exit(0)
	im_file_name = sys.argv[1]

	original = io.imread(im_file_name)
	grayscale = rgb2gray(original)
	print(grayscale.shape)

	num_tile_rows = 102
	num_tile_cols = 173
	blocks = blockshaped(grayscale, num_tile_rows, num_tile_cols)

	base_name = ".".join(sys.argv[1].split('.')[:-1])

	f = open(base_name + "_pix_array.txt", "w")

	blocks = np.mean(blocks, axis=2, keepdims=False)

	for i in range(blocks.shape[0]):
		f.write(" ".join(map(str, blocks[i].tolist())))
		f.write("\n")

	plt.imshow(blocks, cmap="gray")
	plt.savefig(base_name + "_pix_image.png")