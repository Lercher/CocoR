copy /y CasExternalTables.exe \\ntsdtsc\leasing\BIN\CAS
copy /y CasExternalTables.atg \\ntsdtsc\leasing\BIN\CAS
CasExternalTables.exe "\\ntsdtsc\leasing\TEMP\Lercher\UPDATE_EXTERNALTABLES.SQL" > \\ntsdtsc\leasing\BIN\CAS\CasExternalTables.txt