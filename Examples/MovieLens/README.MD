Simple extractor example for movielens http://grouplens.org/datasets/movielens/ extracts.

I have used the 1 million set but should be fairly trivial to use others and change the @INPUTDIR variable.
Local build output for the DLL is set to C:\LocalRunDataRoot, if you have a different Root you will obviously have to change.
This causes an issue with the file "localrundata" that needs to be manually deleted with each build.
If not the error "E_CSC_USER_DDLENTITYALREADYEXISTS: Assembly 'master.ext' already exists." will occurr.

Also not sure if reading from the streams a character at a time is the most optimal either.
