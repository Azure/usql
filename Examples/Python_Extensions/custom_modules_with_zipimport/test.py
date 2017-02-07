REFERENCE ASSEMBLY [ExtPython];

DEPLOY RESOURCE "/modules.zip";

DECLARE @myScript = @"

import sys

sys.path.insert(0, 'modules.zip')
import helloworld

def usqlml_main(df):
    del df['number']
    df['major'] = str(sys.version_info.major)
    df['minor'] = str(sys.version_info.minor)
    df['serial'] = str(sys.version_info.serial)
    df['releaselevel'] = str(sys.version_info.releaselevel)
    df['custommodule'] = str(helloworld.hello_world)
    return df
";

@rows  = 
    SELECT * FROM  (VALUES (1)) AS D(number);

@rows  =
    REDUCE @rows ON number
    PRODUCE major string, minor string, serial string, releaselevel string, custommodule string
    USING new Extension.Python.Reducer(pyScript:@myScript);

OUTPUT @rows
    TO "/usql_python_version.csv"
    USING Outputters.Csv(outputHeader: true);
