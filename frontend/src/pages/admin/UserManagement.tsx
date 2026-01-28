import React, { useEffect, useState } from 'react';
import { Card, Button, Modal, Input } from '../../components/ui/Elements';
import { UserService, User } from '../../services/userService';
import { Plus, Edit2, User as UserIcon, ShieldAlert } from 'lucide-react';

export const UserManagement: React.FC = () => {
    const [users, setUsers] = useState<User[]>([]);
    const [loading, setLoading] = useState(true);
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const [isEditRoleOpen, setIsEditRoleOpen] = useState(false);
    const [selectedUser, setSelectedUser] = useState<User | null>(null);
    const [inviteError, setInviteError] = useState<string | null>(null);
    const [userToBlock, setUserToBlock] = useState<User | null>(null);

    // Form State
    const [formData, setFormData] = useState({
        username: '',
        email: '',
        roles: ['Viewer'],
        authMethod: 'MagicLink', // Default for new invitations
        password: ''
    });

    const loadUsers = async () => {
        setLoading(true);
        try {
            const data = await UserService.getAllUsers();
            setUsers(data);
        } catch (error) {
            console.error('Failed to load users', error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadUsers();
    }, []);

    const handleCreateUser = async () => {
        setInviteError(null);
        try {
            await UserService.inviteUser(formData.email, formData.roles, formData.authMethod);
            setIsCreateOpen(false);
            setFormData({ username: '', email: '', roles: ['Viewer'], password: '', authMethod: 'MagicLink' });
            alert('Invitation sent successfully!');
            loadUsers();
        } catch (error: any) {
            if (error.response && error.response.status === 409) {
                setInviteError(`User ${formData.email} already exists.`);
            } else {
                console.error('Failed to invite user', error);
                setInviteError('Failed to invite user. Please check the email and try again.');
            }
        }
    };

    const handleUpdateRoles = async () => {
        if (!selectedUser) return;
        try {
            await UserService.updateUserRoles(selectedUser.id, formData.roles, formData.authMethod);
            setIsEditRoleOpen(false);
            setSelectedUser(null);
            loadUsers();
        } catch (error) {
            console.error('Failed to update roles', error);
            alert('Failed to update user');
        }
    };

    const handleToggleBlock = (user: User) => {
        setUserToBlock(user);
    };

    const confirmToggleBlock = async () => {
        if (!userToBlock) return;
        try {
            await UserService.updateBlockStatus(userToBlock.id, !userToBlock.isBlocked);
            setUserToBlock(null);
            loadUsers();
        } catch (error) {
            console.error('Failed to update block status', error);
            alert('Failed to update block status');
        }
    };

    const openEditRole = (user: User) => {
        setSelectedUser(user);
        setFormData({
            ...formData,
            username: user.username,
            email: user.email,
            roles: user.roles || [],
            authMethod: user.authMethod || 'MagicLink'
        });
        setIsEditRoleOpen(true);
    };

    const openInviteModal = () => {
        setInviteError(null);
        setIsCreateOpen(true);
    };

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h2 className="text-lg font-medium text-gray-900">User Management</h2>
                    <p className="text-sm text-gray-500">Manage system users and their roles.</p>
                </div>
                <Button onClick={openInviteModal} type="button">
                    <Plus className="h-4 w-4 mr-2" />
                    Invite User
                </Button>
            </div>

            <Card className="overflow-hidden">
                {loading ? (
                    <div className="p-4 text-center text-gray-500">Loading users...</div>
                ) : (
                    <div className="overflow-x-auto">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                                <tr>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">User</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Role</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Auth</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Last Logon</th>
                                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
                                </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                                {users.map((user) => (
                                    <tr key={user.id} className={user.isBlocked ? 'bg-red-50' : ''}>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <div className="flex items-center">
                                                <div className="flex-shrink-0 h-8 w-8 rounded-full bg-gray-100 flex items-center justify-center">
                                                    <UserIcon className="h-4 w-4 text-gray-500" />
                                                </div>
                                                <div className="ml-4">
                                                    <div className="text-sm font-medium text-gray-900">{user.username}</div>
                                                    {user.azureAdObjectId && (
                                                        <div className="text-xs text-blue-600">Linked (Azure AD)</div>
                                                    )}
                                                </div>
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{user.email}</td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <div className="flex flex-wrap gap-1">
                                                {user.roles && user.roles.map(role => (
                                                    <span key={role} className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${role === 'Admin' ? 'bg-purple-100 text-purple-800' :
                                                        role === 'Approver' ? 'bg-blue-100 text-blue-800' :
                                                            role === 'VendorUnit' ? 'bg-orange-100 text-orange-800' :
                                                                'bg-gray-100 text-gray-800'
                                                        }`}>
                                                        {role}
                                                    </span>
                                                ))}
                                            </div>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            {user.authMethod || 'Unknown'}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            {user.isBlocked ? (
                                                <span className="px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-red-100 text-red-800">
                                                    Blocked
                                                </span>
                                            ) : (
                                                <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${user.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' : 'bg-green-100 text-green-800'
                                                    }`}>
                                                    {user.status || 'Active'}
                                                </span>
                                            )}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            {user.lastLogonAt ? new Date(user.lastLogonAt).toLocaleString() : 'Never'}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium flex justify-end gap-2">
                                            <button
                                                onClick={() => openEditRole(user)}
                                                className="text-brand-600 hover:text-brand-900"
                                                title="Edit Role"
                                                type="button"
                                            >
                                                <Edit2 className="h-4 w-4" />
                                            </button>

                                            {/* Resend Invite Option for Pending Users */}
                                            {/* @ts-ignore - status exists but TS might be strict. Checking valid strings */}
                                            {user.status === 'Pending' && (
                                                <button
                                                    onClick={async () => {
                                                        if (confirm(`Resend invitation to ${user.email}?`)) {
                                                            try {
                                                                await UserService.resendInvite(user.email);
                                                                alert('Invitation resent successfully!');
                                                            } catch (e) {
                                                                console.error(e);
                                                                alert('Failed to resend invitation.');
                                                            }
                                                        }
                                                    }}
                                                    className="text-blue-600 hover:text-blue-900"
                                                    title="Resend Invitation"
                                                    type="button"
                                                >
                                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                                                    </svg>
                                                </button>
                                            )}

                                            {user.isBlocked ? (
                                                <button
                                                    onClick={() => handleToggleBlock(user)}
                                                    className="text-green-600 hover:text-green-900"
                                                    title="Unblock User"
                                                    type="button"
                                                >
                                                    <ShieldAlert className="h-4 w-4" />
                                                </button>
                                            ) : (
                                                <button
                                                    onClick={() => handleToggleBlock(user)}
                                                    className="text-red-600 hover:text-red-900"
                                                    title="Block User"
                                                    type="button"
                                                >
                                                    <ShieldAlert className="h-4 w-4" />
                                                </button>
                                            )}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </Card>

            {/* Invite User Modal */}
            <Modal
                isOpen={isCreateOpen}
                onClose={() => setIsCreateOpen(false)}
                title="Invite New User"
                footer={(
                    <div className="flex justify-end gap-2">
                        <Button variant="secondary" onClick={() => setIsCreateOpen(false)} type="button">Cancel</Button>
                        <Button onClick={handleCreateUser} type="button">Send Invitation</Button>
                    </div>
                )}
            >
                <div className="space-y-4">
                    {inviteError && (
                        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded relative" role="alert">
                            <span className="block sm:inline">{inviteError}</span>
                        </div>
                    )}
                    <Input
                        label="Username (Optional)"
                        value={formData.username}
                        onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                    />
                    <Input
                        label="Email"
                        type="email"
                        value={formData.email}
                        onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                    />

                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Authentication Method</label>
                        <select
                            className="block w-full rounded-md border-gray-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm p-2 border"
                            value={formData.authMethod}
                            onChange={(e) => setFormData({ ...formData, authMethod: e.target.value })}
                        >
                            <option value="MagicLink">Magic Link (Vendors - Recommended)</option>
                            <option value="LocalStrong">Local Password + 2FA (Admins)</option>
                            <option value="AzureAd">Azure AD SSO (Staff)</option>
                        </select>
                        <p className="text-xs text-gray-500 mt-1">
                            Unesco.org emails will force Azure AD regardless of selection.
                        </p>
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Roles</label>
                        <div className="space-y-2 border p-3 rounded-md">
                            {['Viewer', 'Requestor', 'Approver', 'VendorUnit', 'BFM', 'Admin'].map((roleOption) => (
                                <label key={roleOption} className="flex items-center">
                                    <input
                                        type="checkbox"
                                        className="h-4 w-4 text-brand-600 focus:ring-brand-500 border-gray-300 rounded"
                                        checked={formData.roles.includes(roleOption)}
                                        onChange={(e) => {
                                            const newRoles = e.target.checked
                                                ? [...formData.roles, roleOption]
                                                : formData.roles.filter(r => r !== roleOption);
                                            setFormData({ ...formData, roles: newRoles });
                                        }}
                                    />
                                    <span className="ml-2 text-sm text-gray-900">{roleOption}</span>
                                </label>
                            ))}
                        </div>
                    </div>
                </div>
            </Modal>

            {/* Edit User Modal */}
            <Modal
                isOpen={isEditRoleOpen}
                onClose={() => setIsEditRoleOpen(false)}
                title={`Edit User: ${selectedUser?.username}`}
                footer={(
                    <div className="flex justify-end gap-2">
                        <Button variant="secondary" onClick={() => setIsEditRoleOpen(false)} type="button">Cancel</Button>
                        <Button onClick={handleUpdateRoles} type="button">Save Changes</Button>
                    </div>
                )}
            >
                <div className="space-y-4">
                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Authentication Method</label>
                        <select
                            className="block w-full rounded-md border-gray-300 shadow-sm focus:border-brand-500 focus:ring-brand-500 sm:text-sm p-2 border"
                            value={formData.authMethod}
                            onChange={(e) => setFormData({ ...formData, authMethod: e.target.value })}
                        >
                            <option value="MagicLink">Magic Link (Vendors - Recommended)</option>
                            <option value="LocalStrong">Local Password + 2FA (Admins)</option>
                            <option value="AzureAd">Azure AD SSO (Staff)</option>
                        </select>
                        <p className="text-xs text-gray-500 mt-1">
                            Use Azure AD for internal staff. Magic Link for external vendors.
                        </p>
                    </div>

                    <div>
                        <label className="block text-sm font-medium text-gray-700 mb-1">Roles</label>
                        <div className="space-y-2 border p-3 rounded-md">
                            {['Viewer', 'Requestor', 'Approver', 'VendorUnit', 'BFM', 'Admin'].map((roleOption) => (
                                <label key={roleOption} className="flex items-center">
                                    <input
                                        type="checkbox"
                                        className="h-4 w-4 text-brand-600 focus:ring-brand-500 border-gray-300 rounded"
                                        checked={formData.roles.includes(roleOption)}
                                        onChange={(e) => {
                                            const newRoles = e.target.checked
                                                ? [...formData.roles, roleOption]
                                                : formData.roles.filter(r => r !== roleOption);
                                            setFormData({ ...formData, roles: newRoles });
                                        }}
                                    />
                                    <span className="ml-2 text-sm text-gray-900">{roleOption}</span>
                                </label>
                            ))}
                        </div>
                    </div>
                </div>
            </Modal>

            {/* Confirmation Modal */}
            <Modal
                isOpen={!!userToBlock}
                onClose={() => setUserToBlock(null)}
                title={userToBlock?.isBlocked ? "Unblock User?" : "Block User?"}
                footer={(
                    <div className="flex justify-end gap-2">
                        <Button variant="secondary" onClick={() => setUserToBlock(null)} type="button">Cancel</Button>
                        <Button
                            onClick={confirmToggleBlock}
                            type="button"
                            className={userToBlock?.isBlocked ? "bg-green-600 hover:bg-green-700" : "bg-red-600 hover:bg-red-700"}
                        >
                            {userToBlock?.isBlocked ? "Confirm Unblock" : "Confirm Block"}
                        </Button>
                    </div>
                )}
            >
                <div className="space-y-4">
                    <div className={`p-4 rounded-md ${userToBlock?.isBlocked ? "bg-green-50 text-green-700" : "bg-red-50 text-red-700"}`}>
                        <p className="font-medium">
                            Are you sure you want to {userToBlock?.isBlocked ? "unblock" : "block"} <strong>{userToBlock?.username}</strong>?
                        </p>
                        <p className="text-sm mt-1">
                            {userToBlock?.isBlocked
                                ? "They will regain access to the portal immediately."
                                : "They will immediately lose access to the portal."}
                        </p>
                    </div>
                </div>
            </Modal>
        </div>
    );
};
