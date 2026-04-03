import { useUpdateGameStatus } from './useCollection';
import { useToast } from '../components';

export function useElectronGameMenu() {
  const updateGameStatus = useUpdateGameStatus();
  const { showToast } = useToast();

  const handleContextMenu = async (
    gameId: string,
    currentStatus: string | null,
    e: React.MouseEvent
  ) => {
    e.preventDefault();
    const electron = (window as any).electron;
    if (!electron) return;
    const result = await electron.showGameMenu(gameId, currentStatus);
    if (!result) return;
    try {
      await updateGameStatus.mutateAsync({ gameId: result.gameId, status: result.status });
    } catch {
      showToast('Failed to update status', 'error');
    }
  };

  return handleContextMenu;
}
