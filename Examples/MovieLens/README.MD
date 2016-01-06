Simple extractor example for movielens http://grouplens.org/datasets/movielens/ csvs.

I have used the 1 million set but should be fairly trivial to use others and change the @INPUTDIR variable.
Local build output for the DLL is set to C:\LocalRunDataRoot if you have a different Root you will obviously have to change.
This causes an issue with the file "localrundata" that needs to be manually deleted with each build.
If not the error master.ext will be shown
