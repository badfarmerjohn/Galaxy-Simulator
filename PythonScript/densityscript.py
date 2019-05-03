from skimage import io
from skimage.color import rgb2gray, rgb2hsv, hsv2rgb
import numpy as np
import matplotlib.pyplot as plt
import sys
import os, shutil

def blockshaped(arr, num_tile_rows, num_tile_cols):
    h, w = arr.shape
    return arr.reshape(num_tile_rows, h // num_tile_rows, num_tile_cols, -1).transpose((0, 2, 1, 3)).reshape(num_tile_rows, num_tile_cols, -1)

def blockshaped_hsv(arr, num_tile_rows, num_tile_cols):
    h, w = arr.shape[0], arr.shape[1]
    return arr.reshape(num_tile_rows, h // num_tile_rows, num_tile_cols, -1, 3).transpose((0, 2, 1, 3, 4)).reshape(num_tile_rows, num_tile_cols, -1, 3)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python {} [image_file]".format(sys.argv[0]))
        exit(0)
    im_file_name = sys.argv[1]

    save_to_current_directory = False

    folder_name = ".".join(sys.argv[1].split('.')[:-1])
    if save_to_current_directory:
        new_folder_path = "./" + folder_name + "/"
    else:
        script_dir = os.path.dirname(__file__)
        new_folder_path = os.path.join(script_dir, "../Assets/Galaxy Files/" + folder_name + "/")

    if os.path.isdir(new_folder_path):
        for f in os.listdir(new_folder_path):
            file_path = os.path.join(new_folder_path, f)
            try:
                if os.path.isfile(file_path):
                    os.unlink(file_path)
            except Exception as e:
                print(e)
    else:
        os.mkdir(new_folder_path)


    original = io.imread(im_file_name)
    grayscale = rgb2gray(original)

    # num_tile_rows = 102
    # num_tile_cols = 173

    num_tile_rows = 714
    num_tile_cols = 865

    blocks = blockshaped(grayscale, num_tile_rows, num_tile_cols)
    
    f_pix = open(new_folder_path + "pix_array.txt", "w")
    mean_blocks = np.mean(blocks, axis=2, keepdims=False)

    for i in range(mean_blocks.shape[0]):
        f_pix.write(" ".join(map(str, mean_blocks[i].tolist())))
        f_pix.write("\n")

    plt.imshow(mean_blocks, cmap="gray")
    plt.savefig(new_folder_path + "pix_image.png")

    hsv_image = rgb2hsv(original)
    blocks_hsv = blockshaped_hsv(hsv_image, num_tile_rows, num_tile_cols)
    
    f_h_mean = open(new_folder_path + "h_mean_array.txt", "w")
    f_s_mean = open(new_folder_path + "s_mean_array.txt", "w")
    f_v_mean = open(new_folder_path + "v_mean_array.txt", "w")

    hsv_mean_blocks = np.mean(blocks_hsv, axis=2, keepdims=False)
    #plt.imshow(hsv2rgb(hsv_mean_blocks))
    #plt.show()

    for i in range(hsv_mean_blocks.shape[0]):
        f_h_mean.write(" ".join(map(str, hsv_mean_blocks[i][:,0].tolist())))
        f_h_mean.write("\n")
        f_s_mean.write(" ".join(map(str, hsv_mean_blocks[i][:,1].tolist())))
        f_s_mean.write("\n")
        f_v_mean.write(" ".join(map(str, hsv_mean_blocks[i][:,2].tolist())))
        f_v_mean.write("\n")

    f_h_std = open(new_folder_path + "h_std_array.txt", "w")
    f_s_std = open(new_folder_path + "s_std_array.txt", "w")
    f_v_std = open(new_folder_path + "v_std_array.txt", "w")

    hsv_std_blocks = np.std(blocks_hsv, axis=2, keepdims=False)

    for i in range(hsv_std_blocks.shape[0]):
        f_h_std.write(" ".join(map(str, hsv_std_blocks[i][:,0].tolist())))
        f_h_std.write("\n")
        f_s_std.write(" ".join(map(str, hsv_std_blocks[i][:,1].tolist())))
        f_s_std.write("\n")
        f_v_std.write(" ".join(map(str, hsv_std_blocks[i][:,2].tolist())))
        f_v_std.write("\n")

    #plt.imshow(hsv2rgb(hsv_std_blocks))
    #plt.show()






