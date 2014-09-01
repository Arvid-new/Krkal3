#pragma once

#ifndef FS_MACROS_H
#define FS_MACROS_H


enum eFSerr{
	eFSgeneral,
	eFSwrite,
	eFSread,
	eFScrc,
	eFSseek,
	eFSflush
};

#define FOPEN(file,mode) ( file[0]!=0 ? fopen(file,mode) : NULL )
#define READ(buf,size,file) if(size&&fread(buf,size,1,file)==0) throw CExc(eFS,eFSread,"Can't read from archive!")
#define WRITE(buf,size,file) if(size&&fwrite(buf,size,1,file)==0) throw CExc(eFS,eFSwrite,"Can't write to archive!")
#define SEEK(file,offset) if(fseek(file,offset,SEEK_SET)!=0) throw CExc(eFS,eFSseek,"Can't seek in archive!")
#define FLUSH(file) if(fflush(file)!=0) throw CExc(eFS,eFSflush,"Can't flush archive!")

//zvetsi na nasobek 32
#define CMP_CEIL(size) ( ( ( (size)+31 ) >>5 ) <<5 )
//pocita skutecnou zabranou velikost souboru (pricte velikost hlavicky a zvetsi na nasobek 32)
#define CMP_FILE_SPACE(filesize) CMP_CEIL(1+4+1+4+filesize)

#endif