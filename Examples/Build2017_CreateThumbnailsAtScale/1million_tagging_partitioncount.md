REFERENCE ASSEMBLY ImageCommon;
REFERENCE ASSEMBLY FaceSdk;
REFERENCE ASSEMBLY ImageEmotion;
REFERENCE ASSEMBLY ImageTagging;
REFERENCE ASSEMBLY ImageOcr;

@imgs = SELECT * FROM Build.dbo.Megaface;

@objects =
    PROCESS @imgs 
    PRODUCE FileName,
            NumObjects int,
            Tags string
    READONLY FileName
    USING new Cognition.Vision.ImageTagger()
    OPTION (PARTITION(FileName) = (PARTITIONCOUNT = 1000));

OUTPUT @objects
    TO "/objects.csv“
     USING Outputters.Csv();
