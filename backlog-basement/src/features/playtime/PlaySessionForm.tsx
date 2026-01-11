import { useState } from 'react';
import { getTodayDateString } from '../../utils';
import { CreatePlaySessionDto } from '../../types';
import './PlaySessionForm.css';

interface PlaySessionFormProps {
  onSubmit: (session: CreatePlaySessionDto) => void;
  isLoading?: boolean;
}

export function PlaySessionForm({ onSubmit, isLoading = false }: PlaySessionFormProps) {
  const [hours, setHours] = useState<string>('');
  const [minutes, setMinutes] = useState<string>('');
  const [datePlayed, setDatePlayed] = useState(getTodayDateString());

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    const totalMinutes = (parseInt(hours) || 0) * 60 + (parseInt(minutes) || 0);

    if (totalMinutes <= 0) return;

    onSubmit({
      durationMinutes: totalMinutes,
      datePlayed: new Date(datePlayed).toISOString(),
    });

    // Reset form
    setHours('');
    setMinutes('');
    setDatePlayed(getTodayDateString());
  };

  return (
    <form onSubmit={handleSubmit} className="play-session-form">
      <div className="form-group">
        <label className="form-label">Duration</label>
        <div className="duration-inputs">
          <div className="duration-input">
            <input
              type="number"
              min="0"
              max="999"
              value={hours}
              onChange={(e) => setHours(e.target.value)}
              placeholder="0"
              className="form-input"
            />
            <span className="duration-label">hours</span>
          </div>
          <div className="duration-input">
            <input
              type="number"
              min="0"
              max="59"
              value={minutes}
              onChange={(e) => setMinutes(e.target.value)}
              placeholder="0"
              className="form-input"
            />
            <span className="duration-label">minutes</span>
          </div>
        </div>
      </div>

      <div className="form-group">
        <label htmlFor="datePlayed" className="form-label">
          Date Played
        </label>
        <input
          type="date"
          id="datePlayed"
          value={datePlayed}
          onChange={(e) => setDatePlayed(e.target.value)}
          max={getTodayDateString()}
          className="form-input"
          required
        />
      </div>

      <button
        type="submit"
        className="btn btn-primary"
        disabled={isLoading || (!hours && !minutes)}
      >
        {isLoading ? 'Logging...' : 'Log Play Session'}
      </button>
    </form>
  );
}
