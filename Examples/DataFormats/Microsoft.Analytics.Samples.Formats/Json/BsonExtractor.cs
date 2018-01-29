// 
// Copyright (c) Microsoft and contributors.  All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// 
// See the License for the specific language governing permissions and
// limitations under the License.
// 
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Analytics.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Microsoft.Analytics.Samples.Formats.Json
{
    /// <summary>
    /// BsonExtractor (sample)
    ///
    ///     [
    ///         { c1:r1v1, c2:r1v2, ...}, 
    ///         { c1:r2v2, c2:r2v2, ...}, 
    ///         ...
    ///     ] 
    ///     => IEnumerable[IRow]
    ///     
    /// </summary>
    [SqlUserDefinedExtractor(AtomicFileProcessing = true)]
    public class BsonExtractor : JsonExtractor
    {
        protected override JsonReader GetJsonReader(Stream stream)
        {
            return (JsonReader)new BsonReader(stream);
        }
    }
}
