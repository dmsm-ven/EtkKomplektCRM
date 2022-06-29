from xls2xlsx import XLS2XLSX
import sys

fileName = sys.argv[1];

x2x = XLS2XLSX(fileName)
x2x.to_xlsx(fileName + "x")