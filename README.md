# Aspell

* Default dictionaries: http://download.services.openoffice.org/contrib/dictionaries
 
* поддерживаемые языки (в файлах допускается солянка из разных языков): Ukr, Rus, Eng
+ поддержка возможности использования дополнительного файла словаря исключений

* 2 формата вывода (интерактивный режим исключён): набор хтмл файлов с содержимым обработанных файлов , где слова с ошибками выделены красным цветом; вывод на консоль контекст и само неправильное слово. (Как компилятор сообщает об ошибках)

* Поддержка проверки синтаксиса в файлах исходного кода (c/cpp/py/Java/h/hpp), где проверки на синтаксис подлежат только блоки комментариев (любых доступных для ЯП) и блоков документирования, при этом спец символы системы документации должны игнорироваться. 
* поддержка проверки синтаксиса для текстовых файлов базовой разметки (adoc, md). При этом, символы разметки должны игнорироваться. Блоки кода (как встроенные так и мультистрочные) должны игнорироваться
* поддержка проверки doc/odt файлов
* возможность запуска под alpine
* возможность расширения функциональности будем создания дополнительных правил, в том числе и для новых типов файлов

# How to launch?

### Command to launch

.\AspellCLI.exe -f "file1.txt"

### Legend of arguments

-f - Required. Path to file, or directory. Can be provided several path with separator ```;```, for example: 
```.\AspellCLI.exe -f "path1;file2.txt;file3.txt"```

--isHtml - Is output result will be in HTML. If not, will be displayed in CLI. Default - false.

-i Input files, from which will take words to ignore. Can be put several by separator ```;```.