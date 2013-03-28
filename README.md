#FreeImage

##MacOS X 10.8.2 iOS6.1

	cd
	mkdir dev
	cd dev
	git clone git@github.com:ninkigumi/freeimage.git

	curl -O http://downloads.sourceforge.net/freeimage/FreeImage3154.zip
	unzip FreeImage3154.zip
	cd FreeImage
	
	cp ../freeimage/Makefile.ios .
	
	make -f Makefile.ios
	lipo -create libfreeimage-iphone.a libfreeimage-iphonesimulator.a -output freeimage.a
	
	