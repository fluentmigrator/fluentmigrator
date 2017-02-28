#region License
// 
// Copyright (c) 2007-2009, Sean Chambers <schambers80@gmail.com>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
#endregion

using System;

namespace FluentMigrator.Tests.Integration
{
    public class IntegrationTestBase
    {
		private const string TestConfigFileName = "TestConfig.xml";

		public void ExecuteWithSupportedProcessor(Action<IMigrationProcessor> test)
        {
            ExecuteWithSupportedProcessor(test, true);
        }

        public void ExecuteWithSupportedProcessor(Action<IMigrationProcessor> test, Boolean tryRollback)
        {
            ExecuteWithSupportedProcessor(test, tryRollback, new Type[] { });
        }

        public void ExecuteWithSupportedProcessor(Action<IMigrationProcessor> test, Boolean tryRollback, params Type[] excludedProcessors)
        {
			var testConfiguration = new TestConfiguration(TestConfigFileName);
			testConfiguration.Configure();
			var testDriver = new TestDriver(testConfiguration.GetProcessorFactory(), testConfiguration.RequestedDbEngine);
			testDriver.Run(test, tryRollback, excludedProcessors);
        }
    }
}