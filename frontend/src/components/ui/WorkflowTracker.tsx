import React from 'react';
import { Check } from 'lucide-react';

interface Step {
  id: string;
  name: string;
  description?: string;
  status: 'complete' | 'current' | 'upcoming' | 'error';
}

interface WorkflowTrackerProps {
  steps: Step[];
}

export const WorkflowTracker: React.FC<WorkflowTrackerProps> = ({ steps }) => {
  return (
    <nav aria-label="Progress">
      <ol role="list" className="overflow-hidden rounded-md border border-gray-200 bg-white lg:flex lg:rounded-none lg:border-0 lg:border-r lg:border-gray-200">
        {steps.map((step, stepIdx) => (
          <li key={step.id} className="relative overflow-hidden lg:flex-1">
            <div
              className={`border-b border-gray-200 overflow-hidden lg:border-0 ${
                stepIdx === 0 ? 'rounded-t-md border-b-0' : ''
              } ${
                stepIdx === steps.length - 1 ? 'rounded-b-md border-b-0' : ''
              } lg:rounded-none`}
            >
              <div className="group">
                <span
                  className="absolute top-0 left-0 h-full w-1 bg-transparent group-hover:bg-gray-200 lg:bottom-0 lg:top-auto lg:h-1 lg:w-full"
                  aria-hidden="true"
                />
                <span
                  className={`px-6 py-5 flex items-start text-sm font-medium ${
                    stepIdx !== 0 ? 'lg:pl-9' : ''
                  }`}
                >
                  <span className="flex-shrink-0">
                    {step.status === 'complete' ? (
                      <span className="flex h-10 w-10 items-center justify-center rounded-full bg-brand-600">
                        <Check className="h-6 w-6 text-white" aria-hidden="true" />
                      </span>
                    ) : step.status === 'current' ? (
                      <span className="flex h-10 w-10 items-center justify-center rounded-full border-2 border-brand-600">
                        <span className="text-brand-600">{stepIdx + 1}</span>
                      </span>
                    ) : step.status === 'error' ? (
                       <span className="flex h-10 w-10 items-center justify-center rounded-full bg-red-600">
                         <span className="text-white font-bold">X</span>
                       </span>
                    ) : (
                      <span className="flex h-10 w-10 items-center justify-center rounded-full border-2 border-gray-300">
                        <span className="text-gray-500">{stepIdx + 1}</span>
                      </span>
                    )}
                  </span>
                  <span className="ml-4 mt-0.5 flex min-w-0 flex-col">
                    <span
                      className={`text-sm font-medium ${
                        step.status === 'complete' || step.status === 'current' ? 'text-brand-600' : 'text-gray-500'
                      }`}
                    >
                      {step.name}
                    </span>
                    <span className="text-sm text-gray-500">{step.description}</span>
                  </span>
                </span>
                
                {stepIdx !== 0 ? (
                  <>
                    <div className="absolute top-0 left-0 hidden h-full w-5 lg:block" aria-hidden="true">
                      <svg
                        className="h-full w-full text-gray-200"
                        viewBox="0 0 22 80"
                        fill="none"
                        preserveAspectRatio="none"
                      >
                        <path
                          d="M0 -2L20 40L0 82"
                          vectorEffect="non-scaling-stroke"
                          stroke="currentcolor"
                          strokeLinejoin="round"
                        />
                      </svg>
                    </div>
                  </>
                ) : null}
              </div>
            </div>
          </li>
        ))}
      </ol>
    </nav>
  );
};