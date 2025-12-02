import React, { useEffect, useState } from 'react';
import { Card, Button } from '../../components/ui/Elements';
import { VendorService } from '../../services/vendorService';
import { Save, Activity, Server, Settings } from 'lucide-react';

export const AdminDashboard: React.FC = () => {
  const [rulesJson, setRulesJson] = useState('');
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    VendorService.getWorkflowRules().then(data => {
      setRulesJson(data);
      setLoading(false);
    });
  }, []);

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">System Administration</h1>
        <p className="mt-1 text-sm text-gray-500">Monitor system health and configure approval workflows.</p>
      </div>

      {/* System Health Indicators */}
      <div className="grid grid-cols-1 gap-5 sm:grid-cols-3">
        <Card className="p-5 flex items-center">
           <div className="rounded-full bg-green-100 p-3 text-green-600">
             <Server className="h-6 w-6" />
           </div>
           <div className="ml-4">
             <h3 className="text-sm font-medium text-gray-500">SAP Connection</h3>
             <p className="text-lg font-bold text-gray-900">Online</p>
           </div>
        </Card>
        <Card className="p-5 flex items-center">
           <div className="rounded-full bg-blue-100 p-3 text-blue-600">
             <Activity className="h-6 w-6" />
           </div>
           <div className="ml-4">
             <h3 className="text-sm font-medium text-gray-500">Logic Apps</h3>
             <p className="text-lg font-bold text-gray-900">Running</p>
           </div>
        </Card>
        <Card className="p-5 flex items-center">
           <div className="rounded-full bg-purple-100 p-3 text-purple-600">
             <Settings className="h-6 w-6" />
           </div>
           <div className="ml-4">
             <h3 className="text-sm font-medium text-gray-500">Active Rules</h3>
             <p className="text-lg font-bold text-gray-900">5</p>
           </div>
        </Card>
      </div>

      {/* Workflow Config Editor */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        <div className="lg:col-span-2">
           <Card title="Workflow Rules Configuration">
             <div className="space-y-4">
               <p className="text-sm text-gray-500">
                 Define approval rules using the JSON configuration below. These rules are processed by the Workflow Engine.
               </p>
               {loading ? (
                 <div className="h-64 bg-gray-100 animate-pulse rounded-md"></div>
               ) : (
                 <textarea 
                   value={rulesJson}
                   onChange={(e) => setRulesJson(e.target.value)}
                   className="w-full h-96 font-mono text-sm p-4 bg-gray-50 border border-gray-300 rounded-md focus:ring-brand-500 focus:border-brand-500"
                   spellCheck={false}
                 />
               )}
               <div className="flex justify-end">
                 <Button>
                   <Save className="mr-2 h-4 w-4" /> Save Configuration
                 </Button>
               </div>
             </div>
           </Card>
        </div>

        <div>
           <Card title="Audit Log Stream">
             <div className="flow-root">
               <ul className="-my-5 divide-y divide-gray-200">
                 {[1, 2, 3, 4].map((i) => (
                   <li key={i} className="py-4">
                     <div className="flex items-center space-x-4">
                       <div className="flex-1 min-w-0">
                         <p className="text-sm font-medium text-gray-900 truncate">
                           Rule Updated
                         </p>
                         <p className="text-sm text-gray-500 truncate">
                           System Admin updated bank approval policy.
                         </p>
                       </div>
                       <div className="inline-flex items-center text-xs font-semibold text-gray-500">
                         2h ago
                       </div>
                     </div>
                   </li>
                 ))}
               </ul>
               <div className="mt-6">
                 <button className="w-full flex justify-center items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50">
                   View Splunk Logs
                 </button>
               </div>
             </div>
           </Card>
        </div>
      </div>
    </div>
  );
};