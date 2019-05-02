from skimage import io
from skimage.color import rgb2gray
import numpy as np
import matplotlib.pyplot as plt
import sys

# def blockshaped(arr, nrows, ncols):
#     h, w = arr.shape
#     return arr.reshape(h // nrows, nrows, -1, ncols).swapaxes(1,2).reshape(-1, nrows, ncols)

def blockshaped(arr, block_height, block_width):
    h, w = arr.shape
    return arr.reshape(block_height, h // block_height, block_width, -1).transpose((0, 2, 1, 3)).reshape(block_height, block_width, -1)


if __name__ == "__main__":
	if len(sys.argv) < 2:
		print("Usage: python {} [image_file]".format(sys.argv[0]))
		exit(0)
	im_file_name = sys.argv[1]

	original = io.imread(im_file_name)
	grayscale = rgb2gray(original)
	print(grayscale.shape)

	block_height = 102
	block_width = 173
	blocks = blockshaped(grayscale, block_height, block_width)

	base_name = ".".join(sys.argv[1].split('.')[:-1])

	f = open(base_name + "_pix_array.txt", "w")

	im = np.zeros((block_height, block_width))

	for i in range(blocks.shape[0]):
		for j in range(blocks.shape[1]):
			avg = np.mean(blocks[i, j])
			im[i, j] = avg
			f.write(str(avg) + " ")
		f.write("\n")

	plt.imshow(im, cmap="gray")
	plt.savefig(base_name + "_pix_image.png")