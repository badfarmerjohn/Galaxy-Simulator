from skimage import io
from skimage.color import rgb2gray
import numpy as np
import matplotlib.pyplot as plt

original = io.imread('galaxy.jpg')
grayscale = rgb2gray(original)
print(grayscale.shape)



'''
fig, axes = plt.subplots(1, 2, figsize=(8, 4))
ax = axes.ravel()

ax[0].imshow(original)
ax[0].set_title("Original")
ax[1].imshow(grayscale, cmap=plt.cm.gray)
ax[1].set_title("Grayscale")

fig.tight_layout()
plt.show()
'''