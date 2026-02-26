import {
    LayoutDashboard,
    User,
    FileText,
    ClipboardList,
    Mail,
    UserPlus,
    Settings,
    ShieldAlert,
    Activity,
    GitBranch,
    Calendar,
    Hexagon,
    DollarSign,
    Share2,
} from 'lucide-react';
import {
    Sidebar,
    SidebarContent,
    SidebarGroup,
    SidebarGroupContent,
    SidebarGroupLabel,
    SidebarMenu,
    SidebarMenuButton,
    SidebarMenuItem,
} from '@/components/ui/sidebar';
import {
    Tooltip,
    TooltipContent,
    TooltipProvider,
    TooltipTrigger,
} from '@/components/ui/tooltip';
import { useAuth, UserRole } from '../context/AuthContext';
import { NavLink } from 'react-router-dom';

// Navigation items for different roles
const VENDOR_NAV = [
    { name: 'Dashboard', href: '/dashboard', icon: LayoutDashboard },
    { name: 'My Reviews', href: '/requests', icon: FileText },
    { name: 'My Profile', href: '/profile', icon: User },
];

const APPROVER_NAV = [
    { name: 'My Worklist', href: '/approver/worklist', icon: ClipboardList },
    { name: 'Invite Vendor', href: '/approver/invite-vendor', icon: Mail },
    { name: 'Event Management', href: '/events', icon: Calendar },
    { name: 'Create Vendor', href: '/approver/create-vendor', icon: UserPlus },
    { name: 'Update Vendor', href: '/approver/select-vendor', icon: FileText },
    { name: 'Request History', href: '/approver/history', icon: FileText },
];

const ADMIN_NAV = [
    { name: 'System Dashboard', href: '/admin/dashboard', icon: LayoutDashboard },
    { name: 'User Management', href: '/admin/users', icon: UserPlus },
    { name: 'System Status', href: '/admin/system-status', icon: Activity },
    { name: 'Workflow Rules', href: '/admin/rules', icon: Settings },
    { name: 'Architecture', href: '/admin/architecture', icon: Hexagon },
    { name: 'Infrastructure Costs', href: '/admin/costs', icon: DollarSign },
    { name: 'Drupal Integration', href: '/admin/integration', icon: Share2 },
    { name: 'Audit Logs', href: '/admin/audit', icon: ShieldAlert },
    { name: 'Branching Strategy', href: '/admin/strategy', icon: GitBranch },
];

export function AppSidebar() {
    const { user } = useAuth();

    // Determine navigation based on role
    let navigation = VENDOR_NAV;
    let roleLabel = 'Vendor Account';

    if (user?.role === 'Admin') {
        navigation = ADMIN_NAV;
        roleLabel = 'System Administrator';
    } else if (['Requestor', 'VendorUnit', 'BFM', 'Approver', 'Viewer'].includes(user?.role || '')) {
        navigation = APPROVER_NAV;
        roleLabel = user?.role === 'Requestor' ? 'Requestor' :
            user?.role === 'VendorUnit' ? 'Vendor Unit Approver' :
                user?.role === 'BFM' ? 'BFM Approver' :
                    user?.role === 'Viewer' ? 'Read-Only Viewer' : 'Internal Approver';
    }

    return (
        <Sidebar collapsible="icon">
            <SidebarContent className="pt-2">
                <SidebarGroup className="py-0">
                    <SidebarGroupLabel className="text-xs px-2 py-2">{roleLabel}</SidebarGroupLabel>
                    <SidebarGroupContent>
                        <SidebarMenu className="gap-1 px-1">
                            {navigation.map((item) => (
                                <SidebarMenuItem key={item.name}>
                                    <Tooltip>
                                        <TooltipTrigger asChild>
                                            <SidebarMenuButton asChild className="h-9 px-2">
                                                <NavLink
                                                    to={item.href}
                                                    className={({ isActive }) =>
                                                        isActive ? 'bg-brand-100 text-brand-900 font-medium' : ''
                                                    }
                                                >
                                                    <item.icon className="h-4 w-4" />
                                                    <span className="group-data-[state=collapsed]:hidden">{item.name}</span>
                                                </NavLink>
                                            </SidebarMenuButton>
                                        </TooltipTrigger>
                                        <TooltipContent side="right" className="hidden group-data-[state=collapsed]:block">
                                            {item.name}
                                        </TooltipContent>
                                    </Tooltip>
                                </SidebarMenuItem>
                            ))}
                        </SidebarMenu>
                    </SidebarGroupContent>
                </SidebarGroup>
            </SidebarContent>
        </Sidebar>
    );
}
