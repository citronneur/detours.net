#include <Windows.h>

int main(int argc, char** argv)
{
	Sleep(2000);
	Sleep(2000);
	Sleep(2000);

	CreateFileW(
		L"bar.txt",
		GENERIC_READ | GENERIC_WRITE,
		0,
		NULL,
		CREATE_NEW,
		FILE_ATTRIBUTE_TEMPORARY,
		NULL);

	return 0;
}