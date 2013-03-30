#FreeImage

##MacOS X 10.8.3 iOS6.1

	cd
	mkdir dev
	cd dev
	git clone git://github.com/ninkigumi/freeimage.git
	
	cd freeimage

	curl -L -O http://downloads.sourceforge.net/project/freeimage/Source%20Distribution/3.15.4/FreeImage3154.zip
	unzip FreeImage3154.zip
	cd FreeImage

	make -f ../Makefile.ios
	lipo -create libfreeimage-iphone.a libfreeimage-iphonesimulator.a -output freeimage.a
	
