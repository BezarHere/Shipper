#pragma once
#include <stdio.h>

static inline void do_stuff(int argc, char *const *argv)
{
	for (int i = 0; i < argc; i++)
	{
		printf("arg [%d] = '%s'\n", i, argv[i]);
	}
}
