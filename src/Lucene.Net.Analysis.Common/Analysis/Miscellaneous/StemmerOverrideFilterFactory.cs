﻿using System.Collections.Generic;
using Lucene.Net.Analysis.Util;
using System.Text.RegularExpressions;
using System.Linq;

namespace Lucene.Net.Analysis.Miscellaneous
{
    /*
     * Licensed to the Apache Software Foundation (ASF) under one or more
     * contributor license agreements.  See the NOTICE file distributed with
     * this work for additional information regarding copyright ownership.
     * The ASF licenses this file to You under the Apache License, Version 2.0
     * (the "License"); you may not use this file except in compliance with
     * the License.  You may obtain a copy of the License at
     *
     *     http://www.apache.org/licenses/LICENSE-2.0
     *
     * Unless required by applicable law or agreed to in writing, software
     * distributed under the License is distributed on an "AS IS" BASIS,
     * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
     * See the License for the specific language governing permissions and
     * limitations under the License.
     */

    /// <summary>
    /// Factory for <seealso cref="StemmerOverrideFilter"/>.
    /// <pre class="prettyprint">
    /// &lt;fieldType name="text_dicstem" class="solr.TextField" positionIncrementGap="100"&gt;
    ///   &lt;analyzer&gt;
    ///     &lt;tokenizer class="solr.WhitespaceTokenizerFactory"/&gt;
    ///     &lt;filter class="solr.StemmerOverrideFilterFactory" dictionary="dictionary.txt" ignoreCase="false"/&gt;
    ///   &lt;/analyzer&gt;
    /// &lt;/fieldType&gt;</pre>
    /// </summary>
    public class StemmerOverrideFilterFactory : TokenFilterFactory, IResourceLoaderAware
    {
        private StemmerOverrideFilter.StemmerOverrideMap dictionary;
        private readonly string dictionaryFiles;
        private readonly bool ignoreCase;

        /// <summary>
        /// Creates a new StemmerOverrideFilterFactory </summary>
        public StemmerOverrideFilterFactory(IDictionary<string, string> args)
            : base(args)
        {
            dictionaryFiles = Get(args, "dictionary");
            ignoreCase = GetBoolean(args, "ignoreCase", false);
            if (args.Count > 0)
            {
                throw new System.ArgumentException("Unknown parameters: " + args);
            }
        }

        public virtual void Inform(IResourceLoader loader)
        {
            if (dictionaryFiles != null)
            {
                AssureMatchVersion();
                IEnumerable<string> files = SplitFileNames(dictionaryFiles);
                if (files.Count() > 0)
                {
                    StemmerOverrideFilter.Builder builder = new StemmerOverrideFilter.Builder(ignoreCase);
                    foreach (string file in files)
                    {
                        IEnumerable<string> list = GetLines(loader, file.Trim());
                        foreach (string line in list)
                        {
                            string[] mapping = new Regex("\t").Split(line, 2);
                            builder.Add(mapping[0], mapping[1]);
                        }
                    }
                    dictionary = builder.Build();
                }
            }
        }

        public virtual bool IgnoreCase
        {
            get
            {
                return ignoreCase;
            }
        }

        public override TokenStream Create(TokenStream input)
        {
            return dictionary == null ? input : new StemmerOverrideFilter(input, dictionary);
        }
    }
}