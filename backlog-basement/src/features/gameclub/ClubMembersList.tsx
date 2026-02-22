import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useToast, FriendButton } from '../../components';
import { useRemoveMember, useUpdateMemberRole } from '../../hooks';
import { GameClubMemberDto } from '../../types';
import './GameClub.css';

interface ClubMembersListProps {
  clubId: string;
  members: GameClubMemberDto[];
  currentUserRole?: string | null;
  currentUserId?: string;
}

export function ClubMembersList({ clubId, members, currentUserRole, currentUserId }: ClubMembersListProps) {
  const [confirmRemove, setConfirmRemove] = useState<string | null>(null);
  const removeMember = useRemoveMember(clubId);
  const updateRole = useUpdateMemberRole(clubId);
  const { showToast } = useToast();

  const canManage = currentUserRole === 'owner' || currentUserRole === 'admin';

  const handleRemove = async (userId: string) => {
    try {
      await removeMember.mutateAsync(userId);
      showToast('Member removed', 'success');
      setConfirmRemove(null);
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to remove member';
      showToast(msg, 'error');
    }
  };

  const handlePromote = async (userId: string, currentRole: string) => {
    const newRole = currentRole === 'member' ? 'admin' : 'member';
    try {
      await updateRole.mutateAsync({ targetUserId: userId, role: newRole });
      showToast(`Role updated to ${newRole}`, 'success');
    } catch (err: unknown) {
      const msg = (err as { response?: { data?: { message?: string } } })?.response?.data?.message ?? 'Failed to update role';
      showToast(msg, 'error');
    }
  };

  return (
    <div className="club-members-list">
      {members.map((member) => (
        <div key={member.userId} className="club-member-row">
          <div className="club-member-info">
            <Link to={`/profile/${member.username}`} className="club-member-name">
              {member.displayName}
            </Link>
            {member.username && (
              <span className="club-member-username">@{member.username}</span>
            )}
            <span className={`club-member-role club-member-role-${member.role}`}>
              {member.role}
            </span>
          </div>
          {member.userId !== currentUserId && (
            <FriendButton userId={member.userId} />
          )}

          {canManage && member.userId !== currentUserId && member.role !== 'owner' && (
            <div className="club-member-actions">
              {currentUserRole === 'owner' && (
                <button
                  className="btn btn-sm btn-secondary"
                  onClick={() => handlePromote(member.userId, member.role)}
                  disabled={updateRole.isPending}
                >
                  {member.role === 'member' ? 'Make Admin' : 'Make Member'}
                </button>
              )}
              {(currentUserRole === 'owner' || (currentUserRole === 'admin' && member.role === 'member')) && (
                confirmRemove === member.userId ? (
                  <div className="club-confirm-row">
                    <span>Remove?</span>
                    <button
                      className="btn btn-sm btn-danger"
                      onClick={() => handleRemove(member.userId)}
                      disabled={removeMember.isPending}
                    >
                      Yes
                    </button>
                    <button
                      className="btn btn-sm btn-secondary"
                      onClick={() => setConfirmRemove(null)}
                    >
                      No
                    </button>
                  </div>
                ) : (
                  <button
                    className="btn btn-sm btn-danger"
                    onClick={() => setConfirmRemove(member.userId)}
                  >
                    Remove
                  </button>
                )
              )}
            </div>
          )}
        </div>
      ))}
    </div>
  );
}
