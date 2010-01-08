all: test

test: ../src/deepzoomit.exe testimage.jpg
	mono ../src/deepzoomit.exe testimage.jpg

../src/deepzoomit.exe:
	make -C ../src

clean:
	rm -rf testimage_files 

